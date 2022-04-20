namespace Gityard.Application.Dtos;

public class ResponseResult<T>
{
    public int Code { get; set; }
    public string? Info { get; set; }
    public T Data { get; set; }
}
