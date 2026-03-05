# ImageForge Proje Kuralları

## C# / .NET Kuralları
- Nullable reference types aktif olacak (<Nullable>enable</Nullable>)
- Async/await pattern her yerde kullanılacak
- Interface-based dependency injection
- Her public method'a XML documentation comment ekle
- Exception'lar custom exception sınıflarıyla fırlatılacak
- Magic string yok, constant veya enum kullan

## Angular Kuralları
- Standalone component mimarisi (NgModule kullanma)
- Signal-based reactivity tercih et (Angular 17+)
- Lazy loading route'lar
- Environment dosyaları ile API URL yönetimi

## Python Kuralları
- Type hints her yerde kullanılacak
- Pydantic model validation
- async/await FastAPI endpoint'leri
- Proper logging (print değil, logging modülü)

## Genel
- Commit mesajları Conventional Commits formatında
- Her servis kendi Dockerfile'ına sahip olacak
- Environment variable'lar .env dosyasından okunacak
- Secret'lar kesinlikle koda yazılmayacak