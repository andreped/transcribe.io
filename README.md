<div align="center">
  <img src="assets/appicon-banner.png" alt="transcribe.io app icon" width="256" height="256"/>
</div>

**transcribe.io** is a cross-platform mobile application built with 
[.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui) and [C#](https://dotnet.microsoft.com/en-us/languages/csharp), enabling private, real-time transcription using on-device AI models.

## Features

- Record audio directly from your microphone on any mobile device
- Transcribe recorded audio and show transcription result using on-device AI model
- Real-time transcription mode (live display of recognized text)
- Model selection and management (download and switch between models)
- Auto-feature to automatically choose which language is spoken

## Tech Stack

- [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui) for cross-platform deployment (iOS, Android)
- [C#](https://dotnet.microsoft.com/en-us/languages/csharp) programming langugage for modern, type-safe application logic
- On-device inference using [Whisper.net](https://github.com/sandrohanea/whisper.net)
- Offering different Whisper variants from [Whisper.cpp](https://huggingface.co/ggerganov/whisper.cpp)

## Getting Started

1. Clone the repository.
2. Open the solution in your preferred IDE (e.g., JetBrains Rider, Visual Studio, VS Code).
3. Build and run the app.

## TODOs
- [x] Implement basic UI with on-device transcription support
- [x] Add support for real-time transcription
- [x] Support downloading and running different Whisper models
- [x] Add iOS deployment support
- [x] Add Android deployment support
- [x] Add support to set transcription language
- [x] Configure CI workflow for basic application testing
- [ ] Configure CI/CD for automated binary builds
- [ ] Improve accuracy on real-time transcriptions

## Acknowledgements

I want to acknowledge [DIPS AS](https://www.dips.com/) for giving me the opportunity to develop this open-source software.  

This project was heavily inspired by [MauiWhisper](https://github.com/drasticactions/MauiWhisper). 
Real-time transcription was based on the work of [whisper_streaming](https://github.com/ufal/whisper_streaming).
