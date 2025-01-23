using Microsoft.AspNetCore.Components.Forms;

namespace DonationBox.BlazorWebAssembly.Models;

public class FileUploadInfo
{
    public IBrowserFile File { get; set; }
    public string FileName { get; set; }
    public string PreviewImage { get; set; }
    public string ExtractedText { get; set; }
    public bool IsExtracting { get; set; }
    public bool IsProcessing { get; set; }
    public bool IsProcessed { get; set; }
    public int UploadProgress { get; set; }
}
