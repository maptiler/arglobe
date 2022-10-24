How to submit new version
*Prerequisities:
1. Visual Studio Community (does not work with Code).
2. Windows 10 SDK.
3. Platform SDK (verified is 10.0.10240.0 but it should work with newer).

*Unity:
1. Set project target platform to "UWP".
2. Set build configuration to "Master".
3. Build.

*Visual Studio:
1. When the Unity build finishes, open the output .sln.
2. Open "Package.appxmanifest" in the main project.
3. Check "Packaging" tab to have the correct publisher certificate.
4. Download the correct certificate if necessary by pressing the "Choose Certificate" button.
5. Pick it by pressing "Select from store" (login to the appropriate Microsoft account with the access to the publishing site is needed) and follow the on-screen instructions.
6. Right click on the main project root -> Publish -> Create App Packages.
7. Pick the "Microsoft Store as <app_name>" option.
8. Enable "ARM" as target architecture.
9. Press "Create" to make the build.

*MS Store:
1. Go to https://partner.microsoft.com/ and sign in.
2. Open "Partner Center" and select "Applications and Games".
3. Pick the application to update from the list.
4. In the list of "Submissions", click on "Update".
5. Go to "Packages".
6. Upload ".appxupload" file from the build folder and press "Save".
7. Press "Send to Store".
8. Wait for the validation and make necessary adjustments in the app when necessary, then repeat the build and upload process.