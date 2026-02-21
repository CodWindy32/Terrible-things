# Быстрый старт - Главное меню

## 🎯 Что нужно сделать (10 минут)

### 1. Создайте MainMenu сцену

1. `File → New Scene` → Сохраните как `MainMenu.unity`
2. Создайте фоновую анимированную сцену (ваши объекты + анимация)
3. Направьте камеру на эту сцену

### 2. Настройте UI

Создайте GameObject `MainMenuUI`:

```
MainMenuUI
├── UI Document (MainMenu.uxml)
├── Volume (Is Global ✅, Weight: 0)
│   └── Add Override → Depth of Field
│       ├── Mode: Bokeh ✅
│       ├── Focus Distance: 0.1 ✅
│       ├── Aperture: 0 ✅
│       └── Focal Length: 50 ✅
├── BlurEffect (Transition Speed: 20, Max Aperture: 10)
└── MainMenuController
    ├── Blur Effect: (ссылка на BlurEffect)
    └── Video Intro Scene Name: "VideoIntro"
```

### 3. Создайте VideoIntro сцену

1. `File → New Scene` → Сохраните как `VideoIntro.unity`
2. Создайте GameObject `VideoPlayer`:

```
VideoPlayer
├── Video Player
│   ├── Video Clip: (ваш MP4 из Assets/Videos/)
│   ├── Render Mode: Camera Far Plane
│   ├── Camera: Main Camera
│   ├── Play On Awake: false
│   └── Loop: false
└── VideoIntroController
    ├── Intro Video: (ваш MP4)
    ├── Game Scene Name: "SampleScene"
    ├── Allow Skip: ✅
    └── Skip Delay: 1
```

### 4. Build Settings

`File → Build Settings`:
- Index 0: MainMenu
- Index 1: VideoIntro
- Index 2: SampleScene

### 5. Добавьте видео

Скопируйте ваш MP4 файл в `Assets/Videos/`

## ✅ Готово!

Запустите MainMenu сцену и проверьте:
- Фон размыт
- Кнопки работают
- Настройки сохраняются
- "Начать игру" → видео → игра

## 🎨 Кастомизация

**Название игры:**
Откройте `MainMenu.uxml`, найдите:
```xml
<ui:Label text="TERRIBLE THINGS" ...
```

**Стили:**
Откройте `MainMenu.uss` и настройте размеры, цвета, spacing.

**Blur:**
В BlurEffect измените Max Aperture (5-20).
