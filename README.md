# ThicknessGage - Разрабатывал для обмена сообщения с контроллером
ПО для обмена сообщениями с другим компьютером, изменения полученного сообщения, записи его в базу данных и отправки изменённого сообщения отправителю

# Платформа
[WPF](https://docs.microsoft.com/ru-ru/visualstudio/designers/getting-started-with-wpf?view=vs-2022) [.NET Framework](https://ru.wikipedia.org/wiki/.NET_Framework) 4.0
<br>
Выбрана именно эта, столь ранняя версия `.NET Framework`, в связи с тем, что на момент разработки на ПК не имелось какой-либо другой

# Что делает?
1.	Получает сообщение
2.	Форматирует получееный массив байт в строку (`<byte><byte><byte>...`)
3.	Записывает в БД
4.	Вытягивает из БД значение и по нему формерует ответное сообщение из полученного ранее (`<byte><byte><byte>...`)
5.  Отправляет сформированную строку

# Сборка
Имеются 3 конфигурации сборки:
-   Debug
    -   Требует запущенного эмулятора `TCP Client Server`
-   Redirector (Предназначен для запуска на сервере)
    -   Требует запущенного на сервере ретранслятора `Redirector` (`Java Channel Proxy`)
    -   Не отправляет сообщения - только получает
-   Release (Предназначен для запуска на сервере)
    -   Подключается напрямую

# NuGet`s
- [NLog](https://nlog-project.org) v5.0.1
- [Oracle.ManagedDataAccess](https://www.oracle.com/database/technologies/appdev/dotnet.html) v.12.1.21
