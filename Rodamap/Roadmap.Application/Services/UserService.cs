using AutoMapper;
using Common.Exceptions;
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
    private readonly IGenericRepository<RefreshToken> _refresh;
    private readonly IGenericRepository<ExpiredToken> _expired;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;


    public UserService(IUserRepository repository, IJwtService jwt, IConfiguration configuration,
        IGenericRepository<RefreshToken> refresh, IMapper mapper, IGenericRepository<ExpiredToken> expired)
    {
        _repository = repository;
        _jwt = jwt;
        _configuration = configuration;
        _refresh = refresh;
        _mapper = mapper;
        _expired = expired;
    }

    public async Task<TokensDto> RegisterUser(RegisterDto registerDto)
    {
        if (await _repository.CheckIfEmailExists(registerDto.Email))
            throw new Conflict("User with this email already exists");

        if (await _repository.CheckIfUsernameExists(registerDto.Username))
            throw new Conflict("User with this usrename already exists");

        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(registerDto.Password, 11);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = registerDto.Email,
            Username = registerDto.Username,
            Password = passwordHash,
        };

        var refreshToken = new RefreshToken
        {
            TokenString = _jwt.GenerateRefreshTokenString(),
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };


        await _repository.CreateAsync(user);
        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.UserId, user.Email, user.Username),
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
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };

        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.UserId, user.Email, user.Username),
            RefreshToken = refreshToken.TokenString
        };
    }

    public async Task<UserDto> GetProfile(Guid userId)
    {
        if (!await _repository.CheckIfIdExists(userId))
            throw new NotFound("User does not exist");

        var user = await _repository.GetById(userId);

        return _mapper.Map<UserDto>(user);
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
            TokenString = accessToken
        });
    }

    public async Task<TokensDto> RefreshToken(TokensDto tokensDto)
    {
        var principal = _jwt.GetTokenPrincipal(tokensDto.AccessToken);

        if (principal == null)
            throw new BadRequest("Invalid access token");
        
        var userIdFromToken = Guid.Parse(principal.FindFirst("UserId")!.Value);

        if (!await _repository.CheckIfIdExists(userIdFromToken))
            throw new NotFound("User doe snot exist");
        
        var user = await _repository.GetById(userIdFromToken);
        
        if (user.RefreshTokens.All(x => x.TokenString != tokensDto.RefreshToken))
            throw new BadRequest("Provided token does not exist or does not belong to this user");
        
        RefreshToken refreshTokenEntity = user.RefreshTokens.FirstOrDefault(x=>x.TokenString == tokensDto.RefreshToken);
        await _refresh.DeleteAsync(refreshTokenEntity!);
        
        
        var refreshToken = new RefreshToken
        {
            TokenString = _jwt.GenerateRefreshTokenString(),
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshDaysLifeTime")),
            User = user
        };

        await _refresh.CreateAsync(refreshToken);

        return new TokensDto
        {
            AccessToken = _jwt.GenerateTokenString(user.UserId, user.Email, user.Username),
            RefreshToken = refreshToken.TokenString
        };
    }
}