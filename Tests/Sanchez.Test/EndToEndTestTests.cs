using Sanchez.Test.Common;

namespace Sanchez.Test;

public abstract class EndToEndTestTests : AbstractTests
{
    protected async Task<string> CreateSampleImagesAsync(FileState state)
    {
        var rootDirectory = state.CreateTempDirectory();

        // Create sample files
        await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"));
        await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T033020Z.jpg"));
        await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T034020Z.jpg"));
        await CreateImage(Path.Combine(rootDirectory, "Overlay.jpg"));

        var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
        await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg"));
        await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T044031Z.jpg"));

        directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
        await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T034100Z.jpg"));
        await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T045100Z.jpg"));
        await CreateImage(Path.Combine(directory.FullName, "IMG_FD_023_IR105_20200830_035006.jpg"));
        await CreateImage(Path.Combine(directory.FullName, "IMG_FD.jpg"));
        await CreateImage(Path.Combine(directory.FullName, "bogus.jpg"));
            
        return rootDirectory;
    }

    protected static void VerifySuccessfulExecution(int returnCode) => returnCode.Should().Be(0, "program should have executed successfully");
    protected static void VerifyFailedExecution(int returnCode) => returnCode.Should().Be(-1, "program should not have executed successfully");        
}