# Steam deployment instructions

First, verify that `context_x86` and `content_x64` directories are empty. Then build two versions of PixiEditor (x86 and x64) using PixiEditor.Builder:

```bash
.\PixiEditor.Builder.exe --project-path "C:\your-own-path-to-repo\PixiEditor\src\PixiEditor" --self-contained -o "C:\your-path-to-repo\PixiEditor\src\SteamBuildScripts\content_x86" --build-configuration Steam --runtime win-x86
```

```bash
.\PixiEditor.Builder.exe --project-path "C:\your-own-path-to-repo\PixiEditor\src\PixiEditor" --self-contained -o "C:\your-path-to-repo\PixiEditor\src\SteamBuildScripts\content_x64" --build-configuration Steam --runtime win-x64
```

Note that the output directories are set to be `content_x64` and `context_x86`. The build results must be in these folders to be picked up by steamcmd (`content_x64` and `context_x86` should directly contain PixiEditor.exe and all the other fluff, don't add a "PixiEditor" subfolder).

After the build had completed, you can change the build description. For that, open BuildScript.vdf and edit the value of "Desc". After that, you are ready to upload the build to steam:

```
"C:\your-steamsdk-location\sdk\tools\ContentBuilder\builder\steamcmd.exe" +login <your-login> <your-password> +run_app_build "C:\your-path-to-repo\PixiEditor\src\SteamBuildScripts\BuildScript.vdf" +quit
```

The password is optional, if you entered it before you can omit it, and steamcmd should pull it from cache. After this has finished running, a new build will appear on the [SteamPipe Builds page](https://partner.steamgames.com/apps/builds/2218560) where you can set it live on some branch.