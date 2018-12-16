# Cutting Force Measurement v1.0.1.1

Приложение позволит Вам записать в реальном времени показания датчиков.
Графический интерфейс создан на основе современной дизайнерской технологии MaterialDesign и системе построения пользовательских приложений WPF

![Главное окно](https://github.com/ortogo/CuttingForceMeasurement/raw/master/screenshots/MainWindow.JPG)

# Используемые библиотеки

* MaterialDesignThemes for WPF - набор стилей для окон
* EPPlus - для создания документа Excel
* Newtonsoft.Json - для работы с JSON

# Инструкция

1. Для начала измерений запустите приложение.
2. Введите ваши данные.
3. В поле для выбора COM выберите тот, к которому подключено устройство. В большинстве случаев он будет выбран по умолчанию. Если оборудование переподключено или подключено позже нажмите кнопку "Обновить" (фиолетовая стрелка по кругу).
4. Запустите станок.
5. Нажмите кнопку "Записать" и получите необходимое количество информации.
6. Нажмите кнопку "Остановить".
7. Сохраните информацию с помощью кнопки "Сохранить". Имя файла будет создано автоматически и расположения сохранения будет рабочий стол. Эти данные можно изменить.
8. После сохранени убедитесь, что все прошло успешно.
9. Для повторной записи можно нажать кнопку "Записать", тогда таблица результатов будет очищена и запись начнется сначала.