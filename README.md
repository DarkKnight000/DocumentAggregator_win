# �������� ����������

Document aggregator - ������ ����������� Web-API ���������� ��� ������������� ����������������.

�����:
������ �������� (anatolii.kostin.99@gmail.com)

����������:
- ������ �� ������������ ��������������\
	������ ������ ������ ���� ����� ����������� � ������������ ������� �������� � ��������� ������. ��� ����� ������ ��� ������� Web-API � ����������� ASP.NET.
- �������������\
	��� ��� ������ �����, ������������� � �����������������, ������������ ����������� ������ ����������, ����������� �������� � ����� ����� � ����� ���������� ����������.\
	����� ����, ������������ ��� ������� ������� ������� "[Clean Architecture](https://fullstackmark.com/post/18/building-aspnet-core-web-apis-with-clean-architecture)", ��������� ������� ����������� ����� ����� ��������, ���������� ������������� � ������������� ����.

---
� ��������� ������� � ������� ���� ����� "������" ��� `request` �������� ������ `claim` ��� ��������� �������� � "�������� ������".

## ������������� ������� - ��������

### ������-�������

- Claim - ������
- Insert - �������

### ������ �������������

��������� ������
- ClaimRequest - ������ �� ������
- ClaimResponse - ����� �� ����������
- ClaimInteractor - ����� ��������� ������

��������� ���������
- FormRequest - ������ �� ���������
- FormResponse - ����� �� ���������
- FormInteractor - ����� ��������� �����

��������� �������
- InsertRequest - ������ �� �������
- InsertResponse - ����� �� �������
- ParseInsertInteractor - ����� ��������� �������

### ����������

��������� ������������
- ILogger - .

������� ������
- IClaimRepository - ����������� �������� `Claim`
- IMixedFieldRepository - ����������� `string` ��� ��������� �� ����� ���� `Insert`
- IEditorService - ������ ��������� ���������

## ������� ���������� - ������ �������������
|||
|-|-|
|���������|������� ����������|
|��������|������ ���� ������ ���������� �� ������� � ��������� � ������� ����������, ����������� ��� ��������� ����������. ����� ��������� ��������������� �������� �� ������.|
|�������� ����������� ����|������ ���� ������|
|��������������� �������|<ul><li>������ �������</li><li>������������ ���� ������ ���������</li></ul>|
|���������������� �������|������ ����� �� ���������� ������|
|�������� �������� ��������|<ol><li>������ �������� HTTP ������ � ������� � ������.</li><li>������� ��������� � ��������� ������ �� ���� ������.</li><li>������� ������������ ���� ������ � ����������� ������� � ��������� ������.</li><li>������� ���������� ����� � ���������� ����� ��� ������ �� ����.</li></ol>|
|����������|<ul><li>(2a) ��� ��������� ������ �� ������.</li><ol><li>������� ���������� ����� � �����, ������� ������ � ��������������� �������.</li></ol><li>(3a) ������� ���� � ������������� ���������.</li><ol><li>������� ���������� ����� � �����, ������� ������ � ��������������� �������.</li></ol></ul>|

����������� �����:
|���� ������ (��������)|���� ������� (����������)|���������|
|-|-|-|
|�����������|�����������|���������.|
|�����������|������������|��������� ������������, �� ���� ��������� ������� ������.<br/>������� ��������������.|
|������������|�����������|���������.<br/>*�������� ������ � ��������� ���.*|
|������������|������������|���������.|

## ������� ��������� �����������

TODO: OracleDatabase

TODO: WordService

## ������� ������������ ���������� ����������

### ������ ������ ASP .NET Core

�������� ��� ������ � ��������� ���������:
```
Content-Type: application/problem+json; charset=utf-8
```

���������� ������ ������� �� ������.

# ������� ������ �� ������� � ���� ����������

## OfficeInterop

������ � ����� ������� �������� ������ � ������� DOCX ��� ���� OfficeInterop.
�� ��������� ��������� ��������� �������������, � ������ ������� ���������� � �������� ����������, �� ������ ���������� ����������, � ������ `DocAggregator.API.Infrastructure.OfficeInterop` ��������.

�������: [������� ��������� ������������� Office](https://support.microsoft.com/ru-ru/topic/%D0%B2%D0%BE%D0%BF%D1%80%D0%BE%D1%81%D1%8B-%D1%81%D0%B5%D1%80%D0%B2%D0%B5%D1%80%D0%BD%D0%BE%D0%B9-%D0%B0%D0%B2%D1%82%D0%BE%D0%BC%D0%B0%D1%82%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8-office-48bcfe93-8a89-47f1-0bce-017433ad79e2)

## �������������� ����� .DOCX �� ����, � ������

����� �� ���� �������, ��� ������������, ��� �������� �� ���������.
����� ��� ������ ���� ��������� �� ����, ���� ���������, ���������� ���, ����� ������� ��� ��� ���������...
������� � � ���������� �������������� ������ � ������ ������ `MemoryStream`.
��, ������� ���������, ��������� �� �� ��� �� ������ ����������� � ������.
�������� ��������� ������ �� � ������ `WordprocessingDocument`, ������� � ��������� ������������ ����� ��� ������ �� ����.
������ ��������� � ����� .NET Core, ������� �� ��� ��������� ������ ������ �� ��� ��������.
�������� ��������� ������ ��������.
�������� ������ � ������ �������� � ������ ������ ������.

- ������ ������ �� ����������� OpenXML SDK: [Calling Save() doesn't flush to the stream � Issue #294 � OfficeDev/Open-XML-SDK � GitHub](https://github.com/OfficeDev/Open-XML-SDK/issues/294)
- ������ ������ �� ����������� .NET: [Add Flush to ZipArchive � Issue #24149 � dotnet/runtime � GitHub](https://github.com/dotnet/runtime/issues/24149)

�� .NET Framework ����� ������ ���.

��� ����� �� ���������� �����: [c# - Open XML WordprocessingDocument with MemoryStream is 0KB - Stack Overflow](https://stackoverflow.com/questions/61196148/open-xml-wordprocessingdocument-with-memorystream-is-0kb)

## ������ ���� � ���� ������

��������, ��� ��� ������ � ��������� ��������� ������ ������� �������������� ����������� ��������.

�������: [Guidance on how to log to a message queue for slow data stores � Issue #11801 � dotnet/AspNetCore.Docs � GitHub](https://github.com/dotnet/AspNetCore.Docs/issues/11801)

# ������ ����

## unoconvert: connection refused

NoConnectException: Connector : couldn't connect to socket (WSAECONNREFUSED, Connection refused)

��������, ������ �� ��������, ������ ��� �� �������.

## unoconvert: type detection failed

IllegalArgumentException: Unsupported URL <file:///C:/bla/bla.docx>: "type detection failed"

�� ��������, �� ����� ������� ���������. �������� ������������� ����������?
