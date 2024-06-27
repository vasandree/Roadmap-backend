using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Roadmap.Application.Dtos.Requests;
using Roadmap.Application.Dtos.Responses;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Application.Interfaces.Services;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IRefreshToken _refresh;
    private readonly IExpiredToken _expired;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;


    public UserService(IUserRepository repository, IRefreshToken refresh, IExpiredToken expired, IJwtService jwt, IConfiguration configuration, IMapper mapper)
    {
        _repository = repository;
        _refresh = refresh;
        _expired = expired;
        _jwt = jwt;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<TokensDto> RegisterUser(RegisterDto registerDto)
    {
        if (await _repository.CheckIfEmailExists(registerDto.Email))
            throw new Conflict("User with this email already exists");

        if (await _repository.CheckIfUsernameExists(registerDto.Username))
            throw new Conflict("User with this username already exists");

        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(registerDto.Password, 11);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = registerDto.Email,
            Username = registerDto.Username,
            Password = passwordHash,
            RefreshTokens = new List<RefreshToken>(),
            CreatedRoadmaps = new List<Domain.Entities.Roadmap>(),
            PrivateAccesses = new List<PrivateAccess>(),
            RecentlyVisited = new List<Guid>(),
            Stared = new List<Guid>()
        };

        var refreshToken = new RefreshToken
        {
            TokenString = _jwt.GenerateRefreshTokenString(),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };


        await _repository.CreateAsync(user);
        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.Id, user.Email, user.Username),
            RefreshToken = refreshToken.TokenString
        };
    }

    public async Task<TokensDto> LoginUser(LoginDto loginDto)
    {
        User user;
        if (await _repository.CheckIfEmailExists(loginDto.Username))
            user = await _repository.GetByEmail(loginDto.Username);

        else if (await _repository.CheckIfUsernameExists(loginDto.Username))
            user = await _repository.GetByUsername(loginDto.Username);
        else
            throw new NotFound("User with this email or username does not exist");

        if (!BCrypt.Net.BCrypt.EnhancedVerify(loginDto.Password, user.Password))
            throw new BadRequest("Wrong password");
        
        var refreshToken = new RefreshToken
        {
            TokenString = _jwt.GenerateRefreshTokenString(),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };

        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.Id, user.Email, user.Username),
            RefreshToken = refreshToken.TokenString
        };
    }

    public async Task<UserDto> GetProfile(Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        var userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }

    public async Task EditProfile(Guid userId, EditProfileDto editProfileDto)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);
        
        if (user.Email != editProfileDto.Email && await _repository.CheckIfEmailExists(editProfileDto.Email))
            throw new Conflict("User with this email already exists");

        if (user.Username != editProfileDto.Username && await _repository.CheckIfUsernameExists(editProfileDto.Username))
            throw new Conflict("User with this username already exists");

        user.Email = editProfileDto.Email;
        user.Username = editProfileDto.Username;

        await _repository.UpdateAsync(user);
    }

    public async Task Logout(Guid userId, string refreshToken, string accessToken)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);
        

        if (user.RefreshTokens.All(x => x.TokenString != refreshToken))
            throw new BadRequest("Provided token does not exist or does not belong to this user");

        RefreshToken refreshTokenEntity = user.RefreshTokens.FirstOrDefault(x=>x.TokenString == refreshToken);

        await _refresh.DeleteAsync(refreshTokenEntity!);

        await _expired.CreateAsync(new ExpiredToken
        {
            Id = Guid.NewGuid(),
            TokenString = accessToken,
            ExpiryDate = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo
        });
    }

    public async Task<TokensDto> RefreshToken(TokensDto tokensDto)
    {
        var principal = _jwt.GetTokenPrincipal(tokensDto.AccessToken);

        if (principal == null)
            throw new BadRequest("Invalid access token");
        
        var userIdFromToken = Guid.Parse(principal.FindFirst("UserId")!.Value);

        if (!await _repository.CheckIfIdExists(userIdFromToken))
            throw new NotFound("User does not exist");
        
        var user = await _repository.GetById(userIdFromToken);
        
        if (user.RefreshTokens != null && user.RefreshTokens.All(x => x.TokenString != tokensDto.RefreshToken))
            throw new BadRequest("Provided token does not exist or does not belong to this user");

        if (user.RefreshTokens != null)
        {
            var refreshTokenEntity = user.RefreshTokens.FirstOrDefault(x=>x.TokenString == tokensDto.RefreshToken);
            await _refresh.DeleteAsync(refreshTokenEntity!);
        }


        var refreshToken = new RefreshToken
        {
            TokenString = _jwt.GenerateRefreshTokenString(),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };

        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.Id, user.Email, user.Username),
            RefreshToken = refreshToken.TokenString
        };
    }

    public async Task ChangePassword(Guid userId, EditPasswordDto editPasswordDto)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);
        
        if (!BCrypt.Net.BCrypt.EnhancedVerify(editPasswordDto.OldPassword, user.Password))
            throw new BadRequest("Wrong password");
        
        user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(editPasswordDto.NewPassword, 11);

        await _repository.UpdateAsync(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsers(Guid? userId, string username)
    {
        var users = await _repository.GetAsQueryable();

        if (!string.IsNullOrEmpty(username))
        {
            users = GetUsersByUsername(users, username);
        }
    
        if (userId.HasValue)
        {
            if (!await _repository.CheckIfIdExists(userId.Value))
            {
                throw new NotFound("User does not exist");
            }

            users = users.Where(u => u.Id != userId.Value);
        }

        return await users.Take(10).Select(u => _mapper.Map<UserDto>(u)).ToListAsync();
    }
    
    private IQueryable<User> GetUsersByUsername(IQueryable<User> users, string? username)
    {
        return !string.IsNullOrEmpty(username) ? users.Where(x=>x.Username.Contains(username)) : users;
    }
}