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
3. Create an `appsettings.Development.json` or `appsettings.Production.json` file and configure it with your Azure Computer Vision and LiteDB settings:
4. Build and run the project:
## Usage

1. Launch the application.
2. Use the OCR functionality to select an image and automatically generate a checklist.
3. Edit and save the checklist as needed.
4. Utilize snackbar notifications and settings for enhanced user experience.

## Key Technologies

- **.NET MAUI**: Cross-platform application framework.
- **LiteDB**: Lightweight NoSQL database.
- **Azure Computer Vision**: Provides OCR functionality.
- **CommunityToolkit.Maui**: UI components and helpers.
- **AdMob**: Ad integration.

## Project Structure

- `CheckListMaker/`: Main application project
  - `Services/`: Business logic and external service integrations
  - `Views/`: UI screens
  - `ViewModels/`: ViewModels following the MVVM pattern
  - `Helpers/`: Helper classes
- `CheckListMakerTest.Tests/`: Unit test project

## Contributing

Bug reports and feature requests are welcome via [Issues](https://github.com/your-username/CheckListMaker/issues). Pull requests are also appreciated.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Third-Party Licenses

This project uses the MTAdmob plugin, which is subject to its own license agreement. Please review the [MTAdmob License Agreement](https://www.nuget.org/packages/Plugin.MauiMTAdmob/2.0.0.5/license) before using this project.

---

We hope you enjoy using this app! Feedback is always welcome.
