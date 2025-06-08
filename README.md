# System Obrony Powietrznej

Symulator systemu obrony powietrznej w konsoli, który demonstruje śledzenie, oznaczanie i zestrzeliwanie celów lotniczych.

## Funkcjonalności

- Symulacja ruchu celów powietrznych w przestrzeni 3D
- Wizualizacja radaru w konsoli (2D)
- Automatyczne wykrywanie i śledzenie celów
- System priorytetyzacji celów na podstawie zagrożenia
- Realistyczna symulacja ruchu celów z prędkością i kierunkiem
- Ograniczenie do jednego celu atakowanego naraz
- Szczegółowe wyświetlanie informacji o celach (pozycja, prędkość, zagrożenie)

## Wymagania

- .NET 8.0 SDK lub nowszy
- Visual Studio 2022 lub nowszy (opcjonalnie)

## Instalacja

1. Sklonuj repozytorium:
```bash
git clone
cd AirDefenseSystem
```

2. Otwórz rozwiązanie w Visual Studio lub skompiluj z linii poleceń:
```bash
dotnet build
```

## Uruchomienie

1. Z linii poleceń:
```bash
cd AirDefenseSystem.Console
dotnet run
```

2. Lub z Visual Studio:
   - Otwórz rozwiązanie `AirDefenseSystem.sln`
   - Ustaw projekt `AirDefenseSystem.Console` jako projekt startowy
   - Naciśnij F5 lub kliknij "Start"

## Sterowanie

- System działa automatycznie
- Naciśnij `Ctrl+C` aby zatrzymać symulację

## Interpretacja wyświetlania

- `R` - pozycja radaru/działka
- Cyfry `0-9` - cele (ostatnia cyfra ID celu)
- `X` - zestrzelone cele
- `.` - puste miejsce

Dla każdego celu wyświetlane są:
- Pozycja (P: x, y, z) w kilometrach
- Prędkość (V: x, y, z) w m/s
- Szybkość (S) w m/s
- Poziom zagrożenia w procentach
- Kierunek ruchu (APPROACHING/MOVING AWAY)

## Struktura projektu

- `AirDefenseSystem.Core` - biblioteka zawierająca główną logikę systemu
  - `Models` - klasy modeli (Target, RadarReading)
  - `Systems` - główne systemy (RadarSystem, AirDefenseSystem)
  - `Utils` - narzędzia pomocnicze (ILogger, ConsoleLogger)
- `AirDefenseSystem.Console` - aplikacja konsolowa z wizualizacją
  - `Display` - klasa odpowiedzialna za wyświetlanie radaru

## Licencja

MIT 