# MapChose

Плагин CounterStrikeSharp для CS2, который циклически переключает сервер между двумя Workshop-картами по таймеру.

## Настройка времени карты

В файле `mapchose.json` укажи время в минутах:

```json
{
  "map_timelimit": 20
}
```

Например, `20` означает, что плагин переключит карту через 20 минут после её запуска. Допустимый диапазон: от 1 до 1440 минут. Если параметр отсутствует или некорректен, используется 20 минут.

Таймер запускается заново при каждой загрузке карты. Плагин не переключает карту при обычном событии завершения раунда или карты — только по своему таймеру.

## Список карт

В `maplist.txt` должно быть ровно две строки с именами карт, которые сервер принимает командой `ds_workshop_changelevel` (не ID коллекции Workshop):

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

Скопируй `MapChose.dll`, `maplist.txt` и `mapchose.json` в:

```text
game/csgo/addons/counterstrikesharp/plugins/MapChose/
```

Затем перезапусти сервер или перезагрузи плагин.

При первом запуске плагин создаст `mapchose.json` и `maplist.txt`, если их нет.
