# CheckListMaker

CheckListMaker is a cross-platform application developed using .NET MAUI. This application allows users to create and manage checklists efficiently. It leverages Azure Computer Vision for OCR functionality and LiteDB for local database management.

## Features

- **Cross-Platform Support**: Runs on Android, iOS, and Windows.
- **OCR Functionality**: Uses Azure Computer Vision to extract text from images and generate checklists automatically.
- **Local Database**: Manages and stores checklists locally using LiteDB.
- **Snackbar Notifications**: Displays notifications using CommunityToolkit.Maui.
- **Ad Integration**: Supports AdMob for displaying ads.
- **Configurable Limits**: Allows setting a limit on the number of checklists stored.

## Requirements

- .NET 8 SDK
- Visual Studio 2022 (with MAUI workload installed)
- Azure Computer Vision API Key and Endpoint

## Installation

1. Clone this repository:
2. Restore the required NuGet packages:
3. Create an `appsettings.Development.json` or `appsettings.Production.json` file and configure it with your Azure AI Vision and LiteDB settings:
4. Build and run the project:
## Usage

1. Launch the application.
2. Use the OCR functionality to select an image and automatically generate a checklist.
3. Edit and save the checklist as needed.
4. Utilize snackbar notifications and settings for enhanced user experience.

## Key Technologies

- **.NET MAUI 9.0**: Cross-platform application framework (.NET 10)
- **LiteDB**: Lightweight NoSQL database
- **Azure AI Vision v4.0**: Provides OCR functionality
- **CommunityToolkit.Maui**: UI components and helpers
- **AdMob**: Ad integration

## Project Structure

- `CheckListMaker/`: Main application project
  - `Services/`: Business logic and external service integrations
  - `Views/`: UI screens
  - `ViewModels/`: ViewModels following the MVVM pattern
  - `Helpers/`: Helper classes
- `CheckListMakerTest.Tests/`: Unit test project

## Recent Updates

### ✨ .NET 10 Upgrade (2025-01)

CheckListMaker has been successfully upgraded to **.NET 10** with the following improvements:

- **Framework**: Upgraded from .NET 8 to .NET 10
- **MAUI**: Upgraded to version 9.0.0
- **Azure AI Vision**: Migrated from deprecated SDK (v7.0.1) to v4.0 SDK (v1.0.0-beta.3)
  - Addresses 2028 deprecation notice
  - Improved security (resolved NU1902 vulnerability)
  - Simplified code (27% reduction in OCR logic)
- **Packages**: All dependencies updated to .NET 10 compatible versions
- **Tests**: 24/24 unit tests passing
- **Security**: Zero known vulnerabilities

For detailed upgrade information, see [`.github/upgrades/UPGRADE_SUMMARY.md`](.github/upgrades/UPGRADE_SUMMARY.md).

## Contributing

Bug reports and feature requests are welcome via [Issues](https://github.com/your-username/CheckListMaker/issues). Pull requests are also appreciated.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Third-Party Licenses

This project uses the MTAdmob plugin, which is subject to its own license agreement. Please review the [MTAdmob License Agreement](https://www.nuget.org/packages/Plugin.MauiMTAdmob/2.0.0.5/license) before using this project.

---

We hope you enjoy using this app! Feedback is always welcome.
