namespace Roadmap.Application.Dtos.Responses;

public class Pagination(int size = 10, int count = 0, int current = 0)
{
    public int Size { get; set; } = size;
    public int Count { get; set; } = count;
    public int Current { get; set; } = current;
}