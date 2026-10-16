@echo off
rem Lance le Boggle web puis ouvre le navigateur sur http://localhost:5090
cd /d "%~dp0"
echo Demarrage du jeu sur http://localhost:5090 (fermez cette fenetre pour l'arreter)...
start "" /b cmd /c ""%SystemRoot%\System32\timeout.exe" /t 6 /nobreak >nul & start http://localhost:5090"
dotnet run --project "%~dp0BoogleGame.Web\BoogleGame.Web.csproj"
pause
