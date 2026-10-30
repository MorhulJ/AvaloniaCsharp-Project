# GymApp Privacy Policy

**Status:** Draft for review before publication. Please confirm that this policy matches the released version of the app and the requirements of the countries where it will be available.

**Effective date:** The date this policy is published.
**Last updated:** October 29, 2026.

## 1. Who is responsible for your data

GymApp is developed and maintained by **Jaroslav Morhul** ("the developer," "we," or "us").

- Privacy contact: **[morhulj@gmail.com](mailto:morhulj@gmail.com)**
- Country: **Poland**

The developer determines why and how GymApp users' data is processed. Google provides the Firebase infrastructure used for authentication and data storage.

## 2. Data the app processes

### Account data

- email address and Firebase account identifier;
- name, if the user provides one;
- password, which Firebase Authentication processes to sign the user in. GymApp does not store the password in its own database.

### Profile and workout data

Depending on what the user enters, this may include gender, date of birth, height, weight, goals and progress, personal records, exercises, workout programs, sets, repetitions, rest periods, supplements, and supplement intake schedules. Gender, date of birth, height, and weight are optional profile details and are stored in Firebase only after the user explicitly consents. If the user declines, account and workout-tracking features remain available. Inputs entered in the Nutrition Calculator are processed on the device and are not saved to Firebase. GymApp stores the consent choice and, when consent is granted, the notice version and time; when consent is withdrawn, it stores the withdrawal time.

When an account is created, GymApp also adds a starter set of example exercises and supplements and a sample workout program. Users can change or delete these examples.

### Technical data

Firebase Authentication may process technical data needed for sign-in, abuse prevention, and service operation, including IP addresses and client information (user-agent). Google describes its processing in its [Firebase privacy information](https://firebase.google.com/support/privacy).

In the current version, the GymApp application code does not integrate advertising SDKs, Google Analytics, Firebase Analytics, or Crashlytics. This statement should be reviewed if new SDKs or services are added.

## 3. How we use data

Data is used to:

- create accounts, sign users in, and maintain sessions;
- display and store profiles, exercises, goals, records, workout programs, and supplement schedules;
- provide app features, including generating a PDF report on the user's device;
- protect accounts and address technical issues reported by users.

Where the GDPR applies, account and workout-tracking data are processed to provide the features the user requests. Optional profile details (gender, date of birth, height, and weight) are stored only after the user gives explicit consent. The app records the consent version and time in the user's Firestore record. Calculator inputs are used locally to calculate estimates and are not saved. Users can withdraw profile-data consent in Profile; GymApp then deletes the saved profile measurements and disables the Nutrition Calculator. Because the legal classification of body and fitness data can depend on context and local law, this approach should be reviewed before worldwide release.

## 4. Where data is stored and who processes it

GymApp uses:

- **Firebase Authentication** to manage sign-in and accounts;
- **Cloud Firestore** to store profiles and the user data listed above.

These services are provided by Google. The storage location for Cloud Firestore is determined by Firebase settings and Google's infrastructure; this policy does not specify a particular database region. The database region does not necessarily determine where all technical data is processed. According to Firebase documentation, Firebase Authentication operates from data centers in the United States, while Cloud Firestore processing depends on its configuration and Google's infrastructure. See [Firebase privacy and data location information](https://firebase.google.com/support/privacy).

Data is transmitted to Firebase services over HTTPS. Google's processing is governed by Google's and Firebase's applicable terms and privacy documents. GymApp does not sell personal data and, in the current version, does not share it with advertising networks.

## 5. Data stored on your device and PDF reports

If a user enables **Remember me**, GymApp stores the email address and a session-restoration token locally in `%AppData%\GymApp\session.dat`. Windows DPAPI encrypts the token for the current Windows user. Signing out deletes the saved session.

When the user selects **Generate report**, GymApp creates a PDF file on the computer's Desktop. It may contain the user's name, goals, personal records, and dates. The file remains on the device and GymApp does not upload it to Firebase. The user must delete it manually when it is no longer needed. Deleting the account does not delete this local PDF.

## 6. Retention and deletion

Account data and associated Firestore data are retained while the account is active or as needed to operate GymApp. Users can use the in-app account deletion feature. It requires password reauthentication and deletes the account and associated data that GymApp stores in Firestore. Users can also withdraw consent for optional profile measurements in Profile; GymApp deletes those measurements and retains the consent status, notice version, grant time, and withdrawal time with the account record.

If deletion fails, some data may not be fully deleted. Please contact us at **[morhulj@gmail.com](mailto:morhulj@gmail.com)**. Google may retain deleted data in backup systems for periods described in Firebase documentation. For example, Google states that Firebase Authentication data is removed from live and backup systems within up to 180 days after the customer initiates deletion. This does not mean that the same retention period applies to every type of Firebase data.

The local session file is deleted when the user signs out. Nutrition Calculator inputs are not saved to Firebase. The user is responsible for deleting PDF reports from the Desktop.

## 7. Your rights

Depending on where you live and which laws apply, you may have the right to access or correct your data, request its deletion or restriction, object to processing, receive a copy of your data, or withdraw consent where processing is based on consent. To make a request, email **[morhulj@gmail.com](mailto:morhulj@gmail.com)**. We may ask you to verify your identity before fulfilling a request.

If you are in the European Economic Area, you may also lodge a complaint with your local data protection authority. In Poland, the authority is the [Urząd Ochrony Danych Osobowych (UODO)](https://uodo.gov.pl/).

## 8. Age and children's data

GymApp currently has no minimum age restriction and may be available in countries around the world. Some countries have specific requirements for processing children's data, including parental or guardian consent. Before an international release, the developer should review these requirements and introduce age restrictions or an appropriate consent process where needed. Requests concerning a child's data can be sent to **[morhulj@gmail.com](mailto:morhulj@gmail.com)**.

## 9. Changes to this policy

We may update this policy if GymApp features, the services we use, or legal requirements change. The current version should be available at the public link listed on the app's Store page. Last updated: **October 29, 2026**.

## 10. Contact

For questions, data requests, or reports of account deletion problems, email **[morhulj@gmail.com](mailto:morhulj@gmail.com)**.

---

## Before publishing

1. Confirm the legal bases for processing and the requirements for data related to health in light of the app's worldwide availability.
2. Because the app has no minimum age, review children's data requirements in target countries and add any safeguards required for those users.
3. Check the final release for SDKs that collect analytics, crash reports, advertising data, or other technical data.
4. Publish this document at a stable public HTTPS URL and add that URL to the GymApp Microsoft Store listing. Microsoft lists a privacy policy URL among app submission information, especially when an app collects personal information.

### Official references

- [Microsoft Learn: Create an MSIX app submission](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/create-app-submission)
- [Microsoft Store Policy: Personal Information](https://learn.microsoft.com/en-us/windows/apps/publish/store-policy-archive/store-policy-7-6)
- [Firebase: Privacy and Security](https://firebase.google.com/support/privacy)
- [GDPR, Article 13 — information to be provided to the data subject](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32016R0679)
