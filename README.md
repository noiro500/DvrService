## Описание
Данный программный продукт представляет из себя службу Windows, которая с помощью ffmpeg получает поток с IP-камер по rtsp протоколу и записывает видео-поток без перекодирования на винчестер компьютера.
Количество камер, с которыз ведется запись, теоретически неограничено и зависит только от скорости работы компьютеных накопителей. Служба позволяет настраивать время записи непрерывного фрагмента видео, 
количество файлов в каталоге, автоматическое удаление старых файлов, перезапуск записи при необходимости. 

## Установка и настройка
1. Скачать и установить ffmpeg.
2. Распоковать скачанные файлы из архива DvrService.zip.
3. Подготовить файл конфигурации ServiceConfig.json.
4. Установить DvrService в качестве службы клмандой (от имени администратора): *sc create DvrService binPath= "{путь к DvrService.exe}\DvrService.exe {путь к файлу ServiceConfig.json}" start= delayed-auto*
5. Если путь путь к файлу ServiceConfig.json не установлен, то служба ищет его по адресу *{каталог windows}\system32\Infrastructure\Config\ServiceConfig.json*
   
## Расшифровка параметров в файле ServiceConfig.json
    FFmpegPath - путь к исполняемому файлу ffmpeg.exe
    CheckOfRecordFilesTimeMin - время в минутах, через которое запускается процедура проверки файлов в папке (в случае сбоя ffmpeg перезапускает его)
    Cameras - список камер с параметрами
        CameraName - описание камеры
        CameraUrl - путь до rtsp потока IP-камеры
        PathRecord - каталог для записи видео. Важно: для каждой камеры каталог должен быть уникальным
        RecordTimeMin - время записи фрагмента видео в минутах
        NumberFilesInFolder - количество файлов в каталоге. Общее время записи видео расчитывается по формуде RecordTimeMin * NumberFilesInFolder
        RemoveOldFilesAfterMin - через какой промежуток времени в минурах запустится процедура удаления страрых файлов
        RestartRecordAfterHours - через какой промежуток времени в часах перезапускать запись видео (необходимо при нестабильном потоке с камер). Значение 0 отключает перезапуск.
