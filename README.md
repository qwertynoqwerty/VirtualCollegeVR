# VirtualCollege

VR-приложение на Unity для знакомства с колледжем и выполнения практических заданий по выбранной специальности. Пользователь проходит обучение с NPC-куратором, получает задания и работает с предметами в виртуальной среде.

Дипломный проект для Meta Quest 3. Он объединяет VR-взаимодействия, диалоги, квесты и серверное хранение профиля, прогресса и достижений.

![Виртуальная среда VirtualCollege](https://raw.githubusercontent.com/qwertynoqwerty/PortfolioSite/main/src/assets/images/college/college-cover.webp)

<p align="center">
  <a href="https://www.youtube.com/watch?v=aH5OWpayVv8">
    <img src="https://img.shields.io/badge/YouTube-%D0%A1%D0%BC%D0%BE%D1%82%D1%80%D0%B5%D1%82%D1%8C%20%D0%B4%D0%B5%D0%BC%D0%BE-FF0000?style=for-the-badge&logo=youtube&logoColor=white" alt="Видео работы приложения">
  </a>
</p>

## Возможности

- Регистрация, авторизация и выбор специальности; отдельная гостевая сцена.
- Пошаговое обучение, диалоги с NPC и подсветка целей.
- Практические задания по информационным системам: сборка компонентов ПК, работа с серверным оборудованием и диагностика.
- Задания по поварскому и кондитерскому делу: сортировка ингредиентов, последовательность действий и выдача блюда.
- Задания по материально-техническому обеспечению: перенос, размещение и сканирование предметов.
- Захват объектов и взаимодействие через VR-контроллеры.
- Профиль, журнал заданий и достижения с загрузкой данных через API.

## Техническая реализация

**Стек:** Unity 6000.0.62f1, C#, Meta XR SDK 85, OpenXR, URP, UnityWebRequest, Unity UI, TextMeshPro.

Код проекта находится в `Assets/_Project/Scripts`. Сетевые клиенты и DTO выделены отдельно от игрового поведения и интерфейса. Практические задания сгруппированы по специальностям.

| Код | Назначение |
| --- | --- |
| [Api/Clients/ApiClient.cs](Assets/_Project/Scripts/Api/Clients/ApiClient.cs) | GET/POST, Bearer-токен, таймаут, отмена запросов и обработка ошибок |
| [Core/Config/ApiEnvironment.cs](Assets/_Project/Scripts/Core/Config/ApiEnvironment.cs) | Конфигурация адреса API |
| [Gameplay/NPCs/NpcQuestRuntime.cs](Assets/_Project/Scripts/Gameplay/NPCs/NpcQuestRuntime.cs) | Состояния задания, диалоги и синхронизация прогресса |
| [Gameplay/Onboarding](Assets/_Project/Scripts/Gameplay/Onboarding) | Обучение и маршрут знакомства с приложением |
| [Gameplay/Quests](Assets/_Project/Scripts/Gameplay/Quests) | Реализация практических VR-заданий |
| [UI](Assets/_Project/Scripts/UI) | Авторизация, журнал, достижения и уведомления |

Квестовый сценарий учитывает начало задания, повторный диалог во время выполнения, выполнение и завершающий диалог. Состояние загружается с сервера. Сетевой клиент возвращает типизированный результат запроса и очищает сохраненный токен при ответе 401.

## Запуск

1. Откройте проект в Unity 6000.0.62f1.
2. Подключите Meta Quest 3 и откройте сцену **AuthScene**.
3. Войдите в учетную запись и пройдите обучение.

Для входа и сохранения прогресса нужен доступный сервер VirtualCollegeApi.

## Связанный проект

[VirtualCollegeApi](https://github.com/qwertynoqwerty/VirtualCollegeApi) - серверная часть приложения. Репозиторий API закрытый; ссылка доступна пользователям с выданным доступом.
