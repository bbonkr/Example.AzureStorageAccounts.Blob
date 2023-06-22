Azure Storage Accounts 의 Blob 항목에 대한 참조를 URI 에서 얻는 코드조각 입니다.

이상하게 한번씩 Blob 항목이 존재하는데, 항목을 찾을 수 없는 현상이 발생해서 확인을 진행합니다.

## 준비

Azure Storage Accounts 리소스를 작성하고, 공유키 등의 구성을 `appsettings.json` 에 입력합니다.

AccountName, AccountKey 또는 ConnectionString 을 입력해야합니다.

## 실행

프로젝트 디렉터리에 있는 `filelist.txt.sample` 파일의 이름을 `filelist.txt` 으로 변경합니다.

확인할 Blob 항목의 URI 를 filelist.txt 파일에 입력합니다.

프로젝트를 실행합니다.

## 확인

여러번 요청해서 확인했으나, Blob 항목이 존재하면, 항목을 찾을 수 없는 현상이 발생하지 않습니다.
