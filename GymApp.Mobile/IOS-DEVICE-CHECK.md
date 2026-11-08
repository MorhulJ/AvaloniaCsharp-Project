# Перевірка GymApp на iPhone

Цільовий пристрій для першої перевірки — iPhone 14 Pro Max з iOS 26.6. Ідентифікатор додатка — `com.morhulj.gymapp`.

## Підготовка збірки

- Для збірки на macOS потрібні .NET 10, workload `ios` і сумісна з цим workload версія Xcode. Звіряйте їх за [вимогами .NET для iOS](https://learn.microsoft.com/dotnet/ios/troubleshooting/xcode-requirement).
- Для прямого встановлення через Xcode потрібні сертифікат Apple Development і профіль із тим самим ідентифікатором додатка та зареєстрованим iPhone. [Інструкція Avalonia](https://docs.avaloniaui.net/docs/deployment/ios)

## Пакет для TestFlight

GitHub Actions запускає **iOS device package** після змін у коді застосунку на гілці `codex/ios-port`. Workflow збирає і перевіряє непідписаний пакет `ios-arm64` без даних Apple.

Для підписаної збірки потрібні сертифікат **Apple Distribution** із приватним ключем у файлі `.p12` і [профіль App Store Connect](https://developer.apple.com/help/account/provisioning-profiles/create-an-app-store-provisioning-profile) для `com.morhulj.gymapp`, створений з тим самим сертифікатом. У репозиторії відкрийте **Settings → Secrets and variables → Actions → New repository secret** і додайте:

| Секрет | Значення |
| --- | --- |
| `IOS_DISTRIBUTION_P12_BASE64` | Вміст файлу `.p12` у Base64 |
| `IOS_DISTRIBUTION_P12_PASSWORD` | Пароль від файлу `.p12` |
| `IOS_APP_STORE_PROFILE_BASE64` | Вміст файлу `.mobileprovision` у Base64 |

У PowerShell скопіюйте Base64 кожного файлу до буфера обміну та вставте його у відповідний секрет. Замініть шлях на шлях до потрібного файлу.

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\path\file.p12')) | Set-Clipboard
```

Коли секрети готові, позначте потрібний коміт у `codex/ios-port` новим тегом і відправте його:

```powershell
git switch codex/ios-port
git pull --ff-only
git tag ios-ipa-1
git push origin ios-ipa-1
```

Workflow підпише пакет і збереже `GymApp.ipa` в артефакті **GymApp-iOS-IPA** на три дні. Артефакт доступний користувачам із правом читати репозиторій. Завантаження в App Store Connect виконується окремо. Для нього створіть запис GymApp з тим самим ідентифікатором. [Підписування в GitHub Actions](https://docs.github.com/en/actions/how-tos/deploy/deploy-to-third-party-platforms/sign-xcode-applications) · [Завантаження збірки в Apple](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/)

## Сценарії на тестовому акаунті

1. Увійдіть із **Keep me signed in**, повністю закрийте додаток і відкрийте його знову. Повторіть без прапорця. Після **Logout** наступний запуск має показувати форму входу.
2. Повторіть запуск із збереженим входом без інтернету. Має з’явитися повідомлення про з’єднання. Відновіть інтернет і перевірте повторний запуск.
3. У світлій і темній темах перевірте верхню панель біля Dynamic Island і нижній край біля смуги жестів.
4. Відкрийте клавіатуру в полі email, числовому полі калькулятора й останньому полі діалогу. Прокрутіть до кнопки збереження. Закрийте клавіатуру й переконайтеся, що нижній відступ повернувся до початкового.
5. У профілі виберіть дату народження, згорніть додаток і поверніться до нього. Перевірте прокручування та введення після повернення.
6. У вкладці **Records** натисніть **Export PDF report**. Збережіть PDF у «Файли» через системне меню та відкрийте його. Звірте ім’я, цілі, рекорди, дати та сторінки. Повторіть експорт і скасуйте меню поширення.

Windows-перегляд підходить для оцінки розміщення елементів і генерації PDF. Сценарії з Keychain, клавіатурою iOS, системними відступами та поширенням файлу перевіряються на iPhone.
