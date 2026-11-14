# MapChose

Плагин CounterStrikeSharp для CS2, который переключает сервер между двумя Workshop-картами по кругу.

## Настройка

Измени `maplist.txt`: оставь ровно две строки с именами карт, которые сервер принимает командой `ds_workshop_changelevel` (не ID коллекции Workshop).

```text
dm_nixware_mirage_d
dm_nixware_fysnow_d
```

Обе карты должны быть заранее доступны серверу через Steam Workshop.

## Сборка

Нужен .NET 8 SDK:

```bash
dotnet restore MapChose.csproj
dotnet build MapChose.csproj -c Release
```

DLL появится в `bin/Release/net8.0/MapChose.dll`.

## Установка

Скопируй `MapChose.dll` и `maplist.txt` в `game/csgo/addons/counterstrikesharp/plugins/MapChose/`, затем перезапусти сервер или перезагрузи плагин.

## Работа

При старте карты плагин запоминает её имя. При завершении выбирает следующую карту из двух настроенных и выполняет `ds_workshop_changelevel <имя_карты>`. После второй карты цикл начинается заново.

> Примечание: DLL здесь не приложена; проект нужно собрать под установленную версию CounterStrikeSharp и проверить на сервере.
