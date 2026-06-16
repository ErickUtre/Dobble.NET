# Dobble.NET

Implementación del juego de cartas **Dobble** con arquitectura **cliente-servidor**:

- **Cliente** (`src/client`): aplicación de escritorio **WPF** (Material Design).
- **Servidor** (`src/server`): servicio **WCF** sobre `net.tcp`, con lógica de negocio y acceso a datos mediante **Entity Framework**.
- **Base de datos**: **SQL Server** (`DobbleBD`).

> .NET Framework **4.7.2** · Windows

---

## Requisitos

| Software | Notas |
|----------|-------|
| **Windows 10/11** | La app usa WPF + WCF (solo Windows). |
| **Visual Studio 2022** (o MSBuild) | Carga de trabajo *.NET desktop development*. |
| **.NET Framework 4.7.2** (Developer Pack) | Framework objetivo del proyecto. |
| **SQL Server Express** | Instancia local (ej. `.\SQLEXPRESS`). Sirve también SQL Server LocalDB/completo. |
| **SSMS o `sqlcmd`** | Para crear/inspeccionar la base de datos (opcional pero recomendado). |

---

## Instalación y ejecución

### 1. Clonar el repositorio

```bash
git clone https://github.com/ErickUtre/Dobble.NET.git
cd Dobble.NET
```

### 2. Crear la base de datos

Ejecuta el script `database/crear_base_de_datos.sql` (crea la base `DobbleBD` y sus tablas).

Con **sqlcmd** (ODBC Driver 18 requiere `-C` para confiar en el certificado):

```powershell
sqlcmd -S ".\SQLEXPRESS" -E -C -i database\crear_base_de_datos.sql
```

O ábrelo en **SQL Server Management Studio** y ejecútalo (F5).

### 3. Configurar la cadena de conexión

Los archivos `CadenaDeConexion.config` **no se suben al repositorio** (contienen datos sensibles), así que debes crearlos a partir de la plantilla `database/CadenaDeConexion.config.template`.

Copia la plantilla a **estas 4 ubicaciones**, renómbrala a `CadenaDeConexion.config` y reemplaza `NOMBRE_SERVIDOR` por tu instancia (ej. `.\SQLEXPRESS`):

- `src/server/DataAccess/CadenaDeConexion.config`
- `src/server/Logica/CadenaDeConexion.config`
- `src/server/Host/CadenaDeConexion.config`
- `tests/Pruebas/CadenaDeConexion.config`

Atajo en PowerShell desde la raíz del repo:

```powershell
$plantilla = "database\CadenaDeConexion.config.template"
$destinos = @(
  "src\server\DataAccess\CadenaDeConexion.config",
  "src\server\Logica\CadenaDeConexion.config",
  "src\server\Host\CadenaDeConexion.config",
  "tests\Pruebas\CadenaDeConexion.config"
)
foreach ($d in $destinos) {
  (Get-Content $plantilla -Raw).Replace("NOMBRE_SERVIDOR", ".\SQLEXPRESS") | Set-Content $d -Encoding utf8
}
```

### 4. Restaurar paquetes NuGet

En Visual Studio se restauran solos al abrir la solución. Por consola:

```powershell
nuget restore Dobble.NET.sln
```

### 5. Compilar

- **Visual Studio**: abre `Dobble.NET.sln` y compila (Ctrl+Shift+B).
- **MSBuild**:

```powershell
msbuild Dobble.NET.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

### 6. Ejecutar

Arranca **primero el servidor** y luego el cliente.

**Desde Visual Studio:** clic derecho en la solución → *Set Startup Projects* → *Multiple startup projects* → pon `Host` y `DobbleGame` en *Start* → F5.

**Por consola** (tras compilar en Debug):

```powershell
Start-Process src\server\Host\bin\Debug\Host.exe      # servidor; espera "El servidor esta corriendo"
Start-Process src\client\bin\Debug\DobbleGame.exe     # cliente (ventana del juego)
```

El servidor escucha en `net.tcp://localhost:8081`. En la ventana del juego usa **Registro** para crear una cuenta y luego inicia sesión.

---

## Estructura del proyecto

```
Dobble.NET/
├── src/
│   ├── client/              # DobbleGame (WPF)
│   │   └── Dominio/         # DTOs locales (CuentaUsuario, Amistad, CuentaUsuarioAmigo)
│   └── server/
│       ├── Host/            # Ejecutable que hospeda el servicio WCF
│       ├── DobbleServicio/  # Contratos e implementación WCF
│       ├── Logica/          # Lógica de negocio
│       ├── DataAccess/      # Entity Framework (ModeloDatosDobble.edmx)
│       ├── Modelo/          # Entidades
│       └── Registro/        # Logging
├── tests/Pruebas/           # Pruebas (MSTest + xUnit)
├── database/
│   ├── crear_base_de_datos.sql
│   └── CadenaDeConexion.config.template
└── Dobble.NET.sln
```

---

## Solución de problemas

| Error | Causa / solución |
|-------|------------------|
| `No se pudo copiar CadenaDeConexion.config` al compilar | Falta crear ese archivo en alguna de las 4 ubicaciones del paso 3. |
| `SSL Provider: la cadena de certificación... no se confía` (sqlcmd) | Agrega `-C` al comando de `sqlcmd` (ODBC Driver 18 cifra por defecto). |
| El servidor cierra con `NetTcpPortSharing ... deshabilitado` | El binding usa `portSharingEnabled`. Para un servidor local déjalo en `false` (config actual), o habilita el servicio como admin: `sc.exe config NetTcpPortSharing start= demand`. |
| El servidor cierra con `HTTP no pudo registrar http://+:8082/` | Registrar un endpoint HTTP requiere permisos de admin (`netsh http add urlacl`). La config actual desactiva el metadato HTTP; el cliente funciona por `net.tcp:8081`. Si lo necesitas, ejecuta el servidor como administrador. |
| El cliente no conecta | Asegúrate de que `Host.exe` esté corriendo **antes** de abrir el cliente. |

---

## Notas

- La configuración de `src/server/Host/App.config` está preparada para ejecutarse **sin permisos de administrador** (sin port sharing ni metadatos HTTP). Para entornos que requieran metadatos WSDL/port sharing, revierte esos ajustes y ejecuta el servidor como administrador.
- Nunca subas tus archivos `CadenaDeConexion.config` (ya están en `.gitignore`).
