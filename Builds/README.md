# Builds

Kodinn's installers, ready to download. **Only installers are here — no source code.**

| Folder | Contents |
|---|---|
| [`windows/`](windows) | the Windows setup (`Kodinn-<version>-setup.exe`) |
| [`windows/msix/`](windows/msix) | the MSIX package and the public certificate (`.cer`) needed to install it |
| [`macos/`](macos) | the macOS package |
| `SHA256SUMS.txt` | the SHA-256 of every file above |

> **Qui ci sono solo gli installer di Kodinn, nessun sorgente.** Il setup per Windows sta in
> `windows/`, il pacchetto MSIX con il suo certificato pubblico in `windows/msix/`, quello per macOS
> in `macos/`. `SHA256SUMS.txt` contiene l'impronta di ciascun file per verificarne l'integrità.

## Downloading

A setup is about 500 MB, more than the 100 MB GitHub accepts for a normal file, so installers are
stored with **[Git LFS](https://git-lfs.com)**:

- from the GitHub website, open the file and press **Download**;
- with git, install Git LFS first (`git lfs install`), then clone: the installers come with it.
  Without Git LFS a clone contains small pointer files instead of the setups.

Each version is also attached to a **[GitHub Release](https://github.com/francescopaolopassaro/Kodinn/releases)**.

## Verifying a download

```powershell
Get-FileHash .\Kodinn-1.2.0-setup.exe -Algorithm SHA256
```

The value must match the line for that file in `SHA256SUMS.txt`.

## Installing the MSIX package

The MSIX is signed with Kodinn's own certificate. Import `windows/msix/kodinn-selfsigned.cer` into
*Trusted Root Certification Authorities* (Local Machine) once, then open the package.
The `.exe` setup needs none of this.

## For maintainers

Kodinn's release build copies the installers here and rewrites the checksums by itself:

```powershell
dotnet build Kodinn.csproj -f net10.0-windows10.0.19041.0 -t:BuildInstaller       # windows/
dotnet build Kodinn.csproj -f net10.0-windows10.0.19041.0 -t:BuildMsixInstaller   # windows/msix/
```

A setup added by hand needs `update-checksums.ps1` afterwards. Then commit and push (Git LFS uploads
the installers), and publish the release:

```powershell
powershell -ExecutionPolicy Bypass -File Builds\publish-release.ps1 -Version 1.2.0
```

Without `-Files` it attaches every installer in this folder plus `SHA256SUMS.txt`.
