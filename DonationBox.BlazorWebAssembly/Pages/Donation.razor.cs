using DonationBox.BlazorWebAssembly.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace DonationBox.BlazorWebAssembly.Pages;

public partial class Donation
{
    private MudForm form;
    private DonationModel donationModel = new DonationModel(); // Model bound to the form
    private List<FileUploadInfo> selectedFiles = new();
    private FileUploadInfo selectedFile;

    [Inject] private HttpClient Http { get; set; }
    private string NotificationMessage { get; set; }
    private string NotificationType { get; set; }
    private bool HasExtractedData => selectedFiles.Any(file => !string.IsNullOrEmpty(file.ExtractedText));
    private string previewImage;
    private string extractedText;

    [Inject] private ISnackbar Snackbar { get; set; }

    //private async Task OnFilesSelected(InputFileChangeEventArgs e)
    //{
    //    selectedFiles.Clear();
    //    NotificationMessage = string.Empty;
    //    foreach (var file in e.GetMultipleFiles())
    //    {
    //        var fileInfo = new FileUploadInfo
    //        {
    //            File = file,
    //            FileName = file.Name,
    //        };

    //        using var stream = file.OpenReadStream();
    //        using var memoryStream = new MemoryStream();
    //        await stream.CopyToAsync(memoryStream);
    //        var base64Image = Convert.ToBase64String(memoryStream.ToArray());
    //        fileInfo.PreviewImage = $"data:image/png;base64,{base64Image}";

    //        selectedFiles.Add(fileInfo);
    //    }
    //}

    private async Task OnImageSelected(InputFileChangeEventArgs e)
    {
        selectedFile = new FileUploadInfo
        {
            File = e.File,
            FileName = e.File.Name,
        };

        // Generate image preview
        using var stream = e.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
       // imagePreview = $"data:{selectedFile.ContentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
    }

    private async Task UploadFiles(IReadOnlyList<IBrowserFile> files)
    {
        selectedFiles.Clear();
        NotificationMessage = string.Empty;
        foreach (var file in files)
        {
            var fileInfo = new FileUploadInfo
            {
                File = file,
                FileName = file.Name,
            };

            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var base64Image = Convert.ToBase64String(memoryStream.ToArray());
            fileInfo.PreviewImage = $"data:image/png;base64,{base64Image}";

            selectedFiles.Add(fileInfo);
        }
    }

    private async Task ExtractData(FileUploadInfo fileInfo)
    {
        int completed = 0;
        if (fileInfo.File != null)
        {
            fileInfo.UploadProgress = 0;
            fileInfo.IsExtracting = true;
            var fileStream = new MemoryStream(); // Create a seekable MemoryStream
            using var stream = fileInfo.File.OpenReadStream();
            await stream.CopyToAsync(fileStream);  // Copy the non-seekable stream to a seekable stream
            fileStream.Seek(0, SeekOrigin.Begin);  // Reset the stream position to the beginning

            try
            {
                // Prepare the form content
                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(fileStream), "file", fileInfo.File.Name);

                var response = await Http.PostAsync("api/uploads/extract-text", formData);
                if (response.IsSuccessStatusCode)
                {
                    donationModel = await response.Content.ReadFromJsonAsync<DonationModel>();
                    selectedFile = fileInfo;
                }
                else
                {
                    fileInfo.ExtractedText = "Failed to extract data.";
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                fileInfo.IsExtracting = false;
                fileInfo.UploadProgress = 100;
                completed++;
                if (completed == selectedFiles.Count)
                {
                    Snackbar.Add($"Data extracted for {fileInfo.FileName} successfully!", Severity.Success);
                    // selectedFiles.Clear();
                }
            }
        }
    }

    private async Task OnValidSubmit(EditContext context)
    {
        if (selectedFile != null)
        {
            selectedFile.IsProcessing = true;
            // Create a seekable MemoryStream for the file
            using var fileStream = new MemoryStream();
            using var stream = selectedFile.File.OpenReadStream();
            await stream.CopyToAsync(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);


            // Prepare the MultipartFormDataContent
            var formData = new MultipartFormDataContent
            {
                // Add the file content
                { new StreamContent(fileStream), "File", selectedFile.FileName },

                // Add the other fields as form data
                { new StringContent(donationModel.CompanyId ?? string.Empty), nameof(donationModel.CompanyId) },
                { new StringContent(donationModel.CompanyName ?? string.Empty), nameof(donationModel.CompanyName) },
                { new StringContent(donationModel.FirstName), nameof(donationModel.FirstName) },
                { new StringContent(donationModel.LastName), nameof(donationModel.LastName) },
                { new StringContent(donationModel.Phone), nameof(donationModel.Phone) },
                { new StringContent(donationModel.Email), nameof(donationModel.Email) },
                { new StringContent(donationModel.StreetAddress ?? string.Empty), nameof(donationModel.StreetAddress) },
                { new StringContent(donationModel.StreetAddress2 ?? string.Empty), nameof(donationModel.StreetAddress2) },
                { new StringContent(donationModel.City ?? string.Empty), nameof(donationModel.City) },
                { new StringContent(donationModel.Region ?? string.Empty), nameof(donationModel.Region) },
                { new StringContent(donationModel.PostalCode ?? string.Empty), nameof(donationModel.PostalCode) },
                { new StringContent(donationModel.Amount), nameof(donationModel.Amount) },
                { new StringContent(donationModel.Message ?? string.Empty), nameof(donationModel.Message) }
            };

            var response = await Http.PostAsync("api/uploads/submit", formData);
            // var response = await Http.PostAsync("api/uploads/submit", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"File {selectedFile.FileName} uploaded successfully. Azure URL: {result}");
            }

            selectedFiles.Remove(selectedFile); // remove selected file

            donationModel = new DonationModel();
            selectedFile.IsProcessing = false;
            selectedFile.IsProcessed = true;
            Snackbar.Add($"Submitted successfully!", Severity.Success);
            selectedFile = null;
            StateHasChanged();
        }
    }

    //private async Task OnValidSubmit(EditContext context)
    //{
    //    foreach (var fileInfo in selectedFiles)
    //    {
    //        using var stream = fileInfo.File.OpenReadStream();
    //        var formData = new MultipartFormDataContent();
    //        formData.Add(new StreamContent(stream), "file", fileInfo.FileName);

    //        var jsonContent = JsonSerializer.Serialize(donationModel); // Convert the object to JSON
    //        var response = await Http.PostAsync("api/uploads/submit", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
    //        if (response.IsSuccessStatusCode)
    //        {
    //            var result = await response.Content.ReadAsStringAsync();
    //            Console.WriteLine($"File {fileInfo.FileName} uploaded successfully. Azure URL: {result}");
    //        }
    //    }
    //    selectedFiles.Clear(); // Clear after submission.

    //    donationModel = new DonationModel();
    //    Snackbar.Add($"Submitted successfully!", Severity.Success);
    //}
}