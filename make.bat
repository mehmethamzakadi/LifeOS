@echo off
REM ============================================
REM LifeOS - Windows Batch Script
REM ============================================
REM Kullanım: make.bat <komut>
REM Yardım: make.bat help
REM ============================================

setlocal enabledelayedexpansion

set "COMPOSE_DEV=docker-compose -f docker-compose.yml -f docker-compose.local.yml"
set "COMPOSE_PROD=docker-compose -f docker-compose.yml -f docker-compose.prod.yml"
set "API_CONTAINER_DEV=lifeos_api_dev"
set "API_CONTAINER_PROD=lifeos_api_prod"
set "DB_CONTAINER_DEV=lifeos_postgres_dev"

if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="dev" goto dev
if "%1"=="dev-up" goto dev-up
if "%1"=="dev-build" goto dev-build
if "%1"=="dev-logs" goto dev-logs
if "%1"=="prod" goto prod
if "%1"=="prod-up" goto prod-up
if "%1"=="prod-build" goto prod-build
if "%1"=="prod-logs" goto prod-logs
if "%1"=="stop" goto stop
if "%1"=="down" goto down
if "%1"=="restart" goto restart
if "%1"=="ps" goto ps
if "%1"=="status" goto status
if "%1"=="logs" goto logs
if "%1"=="logs-api" goto logs-api
if "%1"=="logs-client" goto logs-client
if "%1"=="logs-db" goto logs-db
if "%1"=="migrate" goto migrate
if "%1"=="migrate-up" goto migrate-up
if "%1"=="migrate-down" goto migrate-down
if "%1"=="migrate-list" goto migrate-list
if "%1"=="shell-api" goto shell-api
if "%1"=="shell-db" goto shell-db
if "%1"=="shell-client" goto shell-client
if "%1"=="pull-ollama" goto pull-ollama
if "%1"=="list-ollama" goto list-ollama
if "%1"=="clean" goto clean
if "%1"=="clean-all" goto clean-all
if "%1"=="rebuild" goto rebuild
if "%1"=="test" goto test
if "%1"=="seed" goto seed
goto unknown

:help
echo.
echo ============================================
echo   LifeOS - Windows Komutlari
echo ============================================
echo.
echo Gelistirme Ortami:
echo   make.bat dev          - Development ortamini baslat
echo   make.bat dev-up        - Development ortamini baslat (build olmadan)
echo   make.bat dev-build    - Development servislerini rebuild et
echo   make.bat dev-logs     - Development loglarini izle
echo.
echo Production Ortami:
echo   make.bat prod         - Production ortamini baslat
echo   make.bat prod-up      - Production ortamini baslat (build olmadan)
echo   make.bat prod-build   - Production servislerini rebuild et
echo   make.bat prod-logs    - Production loglarini izle
echo.
echo Servis Yonetimi:
echo   make.bat stop         - Tum servisleri durdur (volume'lar korunur)
echo   make.bat down         - Tum servisleri durdur ve volume'lari sil
echo   make.bat restart      - Tum servisleri yeniden baslat
echo   make.bat ps           - Calisan servisleri listele
echo   make.bat status       - Servis durumlarini goster
echo.
echo Migration Islemleri:
echo   make.bat migrate NAME=MigrationName  - Yeni migration olustur
echo   make.bat migrate-up                  - Migration'lari uygula
echo   make.bat migrate-down                - Son migration'i geri al
echo   make.bat migrate-list                - Migration listesini goster
echo.
echo Log ve Debug:
echo   make.bat logs         - Tum servislerin loglarini izle
echo   make.bat logs-api     - API loglarini izle
echo   make.bat logs-client  - Client loglarini izle
echo   make.bat logs-db      - Database loglarini izle
echo.
echo Container Islemleri:
echo   make.bat shell-api    - API container'ina shell ac
echo   make.bat shell-db     - Database container'ina shell ac
echo   make.bat shell-client - Client container'ina shell ac
echo.
echo Ollama AI:
echo   make.bat pull-ollama MODEL=model-name  - Ollama modelini yukle
echo   make.bat list-ollama                  - Yuklu Ollama modellerini listele
echo.
echo Temizleme:
echo   make.bat clean        - Build cache'leri temizle
echo   make.bat clean-all    - Tum Docker kaynaklarini temizle (DIKKAT!)
echo   make.bat rebuild      - Servisleri rebuild et ve baslat
echo.
goto end

:dev
call :dev-build
call :dev-up
echo.
echo [OK] Development ortami baslatildi!
echo Frontend: http://localhost:5173
echo Backend API: http://localhost:6060
echo API Docs: http://localhost:6060/scalar/v1
echo Seq Logs: http://localhost:5341
echo Jaeger: http://localhost:16686
goto end

:dev-up
echo [INFO] Development ortami baslatiliyor...
%COMPOSE_DEV% up -d
if %ERRORLEVEL% EQU 0 (
    echo [OK] Servisler baslatildi
) else (
    echo [HATA] Servisler baslatilamadi!
    exit /b 1
)
goto end

:dev-build
echo [INFO] Development servisleri build ediliyor...
%COMPOSE_DEV% build --no-cache
if %ERRORLEVEL% EQU 0 (
    echo [OK] Build tamamlandi
) else (
    echo [HATA] Build basarisiz!
    exit /b 1
)
goto end

:dev-logs
%COMPOSE_DEV% logs -f
goto end

:prod
call :prod-build
call :prod-up
echo.
echo [OK] Production ortami baslatildi!
echo ONEMLI: Production ortami icin .env dosyasini kontrol edin!
goto end

:prod-up
echo [INFO] Production ortami baslatiliyor...
if not exist .env (
    echo [HATA] .env dosyasi bulunamadi!
    echo Production icin .env dosyasi olusturun: copy .env.example .env
    exit /b 1
)
%COMPOSE_PROD% up -d
if %ERRORLEVEL% EQU 0 (
    echo [OK] Production servisleri baslatildi
) else (
    echo [HATA] Servisler baslatilamadi!
    exit /b 1
)
goto end

:prod-build
echo [INFO] Production servisleri build ediliyor...
%COMPOSE_PROD% build --no-cache
if %ERRORLEVEL% EQU 0 (
    echo [OK] Build tamamlandi
) else (
    echo [HATA] Build basarisiz!
    exit /b 1
)
goto end

:prod-logs
%COMPOSE_PROD% logs -f
goto end

:stop
echo [INFO] Servisler durduruluyor (volume'lar korunacak)...
docker ps | findstr lifeos >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    %COMPOSE_DEV% down 2>nul
    %COMPOSE_PROD% down 2>nul
    echo [OK] Servisler durduruldu (volume'lar korundu)
) else (
    echo [INFO] Calisan servis bulunamadi
)
goto end

:down
echo [DIKKAT] Tum servisler ve volume'lar silinecek!
set /p confirm="Devam etmek istediginize emin misiniz? (y/N): "
if /i not "!confirm!"=="y" (
    echo Islem iptal edildi
    goto end
)
echo [INFO] Servisler ve volume'lar siliniyor...
%COMPOSE_DEV% down -v 2>nul
%COMPOSE_PROD% down -v 2>nul
echo [OK] Tum servisler ve volume'lar silindi
goto end

:restart
call :stop
call :dev-up
echo [OK] Servisler yeniden baslatildi
goto end

:ps
echo [INFO] Calisan servisler:
docker ps --filter "name=lifeos" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
goto end

:status
echo [INFO] Development Servis Durumlari:
%COMPOSE_DEV% ps 2>nul || echo Development servisleri calismiyor
echo.
echo [INFO] Production Servis Durumlari:
%COMPOSE_PROD% ps 2>nul || echo Production servisleri calismiyor
goto end

:logs
%COMPOSE_DEV% logs -f
goto end

:logs-api
%COMPOSE_DEV% logs -f lifeos.api
goto end

:logs-client
%COMPOSE_DEV% logs -f lifeos.client
goto end

:logs-db
%COMPOSE_DEV% logs -f postgresdb
goto end

:migrate
if "%NAME%"=="" (
    echo [HATA] Migration adi belirtilmedi!
    echo Kullanim: make.bat migrate NAME=MigrationName
    exit /b 1
)
echo [INFO] Migration olusturuluyor: %NAME%
echo [INFO] Not: Migration dosyalari container icinde olusturulur. Host'a kopyalamak icin volume mount gerekir.
docker exec -it %API_CONTAINER_DEV% dotnet ef migrations add %NAME% --project /app/LifeOS.Persistence.csproj --output-dir Migrations/PostgreSql --context LifeOSDbContext
if %ERRORLEVEL% EQU 0 (
    echo [OK] Migration olusturuldu: %NAME%
    echo [INFO] Migration dosyalarini gormek icin: docker exec -it %API_CONTAINER_DEV% ls -la /app/Migrations/PostgreSql
) else (
    echo [HATA] Migration olusturulamadi! Container calisiyor mu?
    exit /b 1
)
goto end

:migrate-up
echo [INFO] Migration'lar uygulaniyor...
docker exec -it %API_CONTAINER_DEV% dotnet ef database update --project /app/LifeOS.Persistence.csproj --context LifeOSDbContext
if %ERRORLEVEL% EQU 0 (
    echo [OK] Migration'lar uygulandi
) else (
    echo [HATA] Migration'lar uygulanamadi! Container calisiyor mu?
    exit /b 1
)
goto end

:migrate-down
echo [DIKKAT] Son migration geri alinacak!
set /p confirm="Devam etmek istediginize emin misiniz? (y/N): "
if /i not "!confirm!"=="y" (
    echo Islem iptal edildi
    goto end
)
echo [INFO] Son migration geri aliniyor...
docker exec -it %API_CONTAINER_DEV% dotnet ef migrations remove --project /app/LifeOS.Persistence.csproj --context LifeOSDbContext
if %ERRORLEVEL% EQU 0 (
    echo [OK] Migration geri alindi
) else (
    echo [HATA] Migration geri alinamadi! Container calisiyor mu?
    exit /b 1
)
goto end

:migrate-list
echo [INFO] Migration Listesi:
docker exec -it %API_CONTAINER_DEV% dotnet ef migrations list --project /app/LifeOS.Persistence.csproj --context LifeOSDbContext
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Migration listesi alinamadi. Container calisiyor mu?
    exit /b 1
)
goto end

:shell-api
echo [INFO] API container'ina baglaniliyor...
docker exec -it %API_CONTAINER_DEV% /bin/sh
if %ERRORLEVEL% NEQ 0 (
    docker exec -it %API_CONTAINER_DEV% /bin/bash
    if %ERRORLEVEL% NEQ 0 (
        echo [HATA] Container calismiyor veya shell bulunamadi
        exit /b 1
    )
)
goto end

:shell-db
echo [INFO] Database container'ina baglaniliyor...
docker exec -it %DB_CONTAINER_DEV% psql -U postgres -d LifeOSDb
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Database container calismiyor
    exit /b 1
)
goto end

:shell-client
echo [INFO] Client container'ina baglaniliyor...
docker exec -it lifeos_client_dev /bin/sh
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Client container calismiyor
    exit /b 1
)
goto end

:pull-ollama
if "%MODEL%"=="" set "MODEL=qwen2.5:7b"
echo [INFO] Ollama modeli yukleniyor: %MODEL%
docker exec -it lifeos_ollama_dev ollama pull %MODEL%
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Model yuklenemedi. Ollama container calisiyor mu?
    exit /b 1
)
goto end

:list-ollama
echo [INFO] Yuklu Ollama Modelleri:
docker exec -it lifeos_ollama_dev ollama list
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Ollama container calismiyor
    exit /b 1
)
goto end

:clean
echo [INFO] Build cache'leri temizleniyor...
docker system prune -f
docker builder prune -f
echo [OK] Temizleme tamamlandi
goto end

:clean-all
echo [DIKKAT] Tum Docker kaynaklari silinecek!
set /p confirm="Devam etmek istediginize emin misiniz? (y/N): "
if /i not "!confirm!"=="y" (
    echo Islem iptal edildi
    goto end
)
echo [INFO] Tum Docker kaynaklari temizleniyor...
docker system prune -a --volumes -f
echo [OK] Tum Docker kaynaklari temizlendi
goto end

:rebuild
call :clean
call :dev-build
call :dev-up
echo [OK] Servisler rebuild edildi ve baslatildi
goto end

:test
echo [INFO] Testler calistiriliyor...
docker exec -it %API_CONTAINER_DEV% dotnet test
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Testler calistirilamadi. Container calisiyor mu?
    exit /b 1
)
goto end

:seed
echo [INFO] Database seed islemi calistiriliyor...
echo [INFO] Not: Seed islemi API baslatildiginda otomatik calisir
docker exec -it %API_CONTAINER_DEV% dotnet run --project /app/LifeOS.API.csproj
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Seed islemi calistirilamadi
    exit /b 1
)
goto end

:unknown
echo [HATA] Bilinmeyen komut: %1
echo Yardim icin: make.bat help
exit /b 1

:end
endlocal

