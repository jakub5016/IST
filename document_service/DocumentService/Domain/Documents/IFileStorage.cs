namespace DocumentService.Domain.Documents
{
    public interface IFileStorage
    {
        Task UploadFile(Stream file, string path);
        Task<string> GetFile(string path);
    }
}
