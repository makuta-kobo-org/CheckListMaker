# Changelog

All notable changes to CheckListMaker will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed - 2025-01

#### Framework & SDK Upgrades
- **Upgraded to .NET 10** from .NET 8
  - CheckListMaker: `net10.0;net10.0-android`
  - CheckListMakerTest.Tests: `net10.0`
- **Upgraded MAUI to 9.0.0** from 8.0.100
  - Full .NET 10 compatibility
  - Latest UI rendering improvements

#### Azure AI Vision Migration
- **Migrated to Azure AI Vision v4.0 SDK** from deprecated v7.0.1
  - Package: `Azure.AI.Vision.ImageAnalysis` v1.0.0-beta.3
  - Addresses Microsoft's 2028 deprecation notice
  - Simplified OCR implementation (27% code reduction)
  - Removed polling logic for better performance

#### Package Updates
- Updated all `Microsoft.Extensions.*` packages to 10.0.2
- Updated `Newtonsoft.Json` to 13.0.4

#### Removed
- Removed `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` (deprecated)
- Removed `System.Private.Uri` (integrated into .NET 10 framework)
- Removed `System.Text.RegularExpressions` (integrated into .NET 10 framework)

### Security
- **Resolved NU1902 vulnerability** in `Microsoft.Rest.ClientRuntime`
- Zero known vulnerabilities after upgrade

### Testing
- All 24 unit tests passing
- Verified on Android emulator
- OCR functionality tested and working with new SDK

---

## Migration Details

For detailed migration information, see:
- [Upgrade Plan](.github/upgrades/plan.md)
- [Assessment](.github/upgrades/assessment.md)
- [Progress & Summary](.github/upgrades/progress.md)
- [Upgrade Summary](.github/upgrades/UPGRADE_SUMMARY.md)

---

## Previous Versions

### [1.0.0] - Initial Release
- Initial release with .NET 8 and MAUI 8.0
- Azure Computer Vision OCR integration
- LiteDB local database
- Cross-platform support (Android, iOS, Windows)
