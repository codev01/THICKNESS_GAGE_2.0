rem удаляем файлы старше N дней 
rem /D -14
forfiles /p .\Logs /S /D -14 /C "cmd /c del /f /q /a @file" 