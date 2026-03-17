"""
Additive Gantt updater.
- Reads existing Duration/Start/Finish from ChirbitsplanORGININAL.xml
- ADDS diary hours to the existing planned hours
- Start = min(original_start, diary_start)
- Finish = max(original_finish, diary_finish_based_on_accumulated_hours)
- Uses safe position-based patching (no DOTALL regex)
"""
import xml.etree.ElementTree as ET
from datetime import datetime, timedelta
import re

def parse_dur(s):
    """Parse PT#H#M0S -> float hours"""
    m = re.match(r'PT(\d+)H(\d+)M', s)
    if m:
        return int(m.group(1)) + int(m.group(2)) / 60.0
    return 0.0

def fmt_dur(hours):
    h = int(hours)
    m = int(round((hours - h) * 60))
    return f"PT{h}H{m}M0S"

def fmt_dt(dt):
    return dt.strftime("%Y-%m-%dT%H:%M:00")

def patch_block(block, patch):
    block = re.sub(r'<Start>[^<]*</Start>',    f'<Start>{patch["start"]}</Start>',        block)
    block = re.sub(r'<Finish>[^<]*</Finish>',  f'<Finish>{patch["finish"]}</Finish>',      block)
    block = re.sub(r'<Duration>[^<]*</Duration>', f'<Duration>{patch["duration"]}</Duration>', block)
    block = re.sub(r'<Resume>[^<]*</Resume>',  f'<Resume>{patch["resume"]}</Resume>',      block)
    block = re.sub(r'<RemainingDuration>[^<]*</RemainingDuration>',
                   '<RemainingDuration>PT0H0M0S</RemainingDuration>', block)
    block = re.sub(r'<PercentComplete>[^<]*</PercentComplete>',
                   '<PercentComplete>100</PercentComplete>', block)
    block = re.sub(
        r'(<ExtendedAttribute>\s*<FieldID>188743783</FieldID>\s*<Value>)[^<]*(</Value>)',
        rf'\g<1>{patch["duration"]}\g<2>', block)
    return block

def apply_patch(content, task_name, patch):
    needle    = f'<Name>{task_name}</Name>'
    pos       = content.find(needle)
    if pos == -1:
        print(f"  [MISS] {task_name}")
        return content, False
    open_tag  = content.rfind('<Task>', 0, pos)
    close_tag = content.find('</Task>', pos) + len('</Task>')
    block     = content[open_tag:close_tag]
    content   = content[:open_tag] + patch_block(block, patch) + content[close_tag:]
    return content, True

# ─── DIARY SESSIONS ────────────────────────────────────────────────────────────
sessions = [
    # ──────────────────────── FEBRERO ────────────────────────────────────────
    # Día 17: revisión RFTP + Gantt + borrador memoria (8:30-14:30, 6h)
    ("2026-02-17","08:30","14:30", [
        "R07F04T01 – Realizar borrador de la memoria",
        "R07F04T01P01 – Verificar que cumpla con el formato solicitado",
    ]),
    # Día 18: UI app móvil registro + análisis conexión (8:30-14:30, 6h)
    ("2026-02-18","08:30","14:30", [
        "R01F01T03 – Diseñar interfaz de registro en app móvil",
        "R01F01T03P01 – Visualizar pantalla de registro y validar campos obligatorios",
    ]),
    # Día 19: endpoint registro (8:30-11:30, 3h) + Firebase/socket (11:30-14:30, 3h)
    ("2026-02-19","08:30","11:30", [
        "R01F01T02 – Implementar endpoint o servicio de registro",
        "R01F01T02P01 – Enviar solicitud POST desde app móvil y verificar respuesta exitosa",
    ]),
    ("2026-02-19","11:30","14:30", [
        "R02F03T01 – Definir sistema de conexión a utilizar sockets WebSockets o Bluetooth",
        "R02F03T02 – Implementar sincronización de estados mediante sockets WebSockets o Bluetooth",
        "R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador",
    ]),
    # Día 20: lobby/spawn (8:30-14:30, 6h)
    ("2026-02-20","08:30","14:30", [
        "R02F01T01 – Implementar servidor de sesiones usando Unity",
        "R02F01T01P01 – Crear lobby y obtener código único de acceso",
    ]),
    # Día 24: refactorizar + entidad relación + diagrama componentes (8:30-11:00, ~2.5h)
    ("2026-02-24","08:30","11:00", [
        "R07F01T01 – Elaborar diagrama de componentes",
        "R07F01T01P01 – Validar coherencia del flujo de datos",
        "R07F01T02 – Detallar descripción de arquitectura",
        "R07F03T01 – Crear entidad relación",
        "R07F03T01P01 – Verificar que sea coherente con las entidades que requiere el sistema",
    ]),
    # Día 25: interconexión clave numérica 6 dígitos + estilizado app (8:30-14:30, 6h)
    # Also: lobbies selection rules Firebase
    ("2026-02-25","08:30","14:30", [
        "R02F03T02 – Implementar sincronización de estados mediante sockets WebSockets o Bluetooth",
        "R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador",
        "R02F02T02 – Validar límite máximo de jugadores",
        "R02F02T02P01 – Intentar ingreso cuando el lobby esté lleno",
    ]),
    # ──────────────────────── MARZO ──────────────────────────────────────────
    # Día 02/03: mejoras/bugs app + interconexión minijuego prueba + cambio escenas (8:30-14:30, 6h)
    ("2026-03-02","08:30","14:30", [
        "R02F02T01 – Implementar pantalla de ingreso de código en app móvil",
        "R02F02T01P01 – Introducir código válido y verificar conexión al lobby",
    ]),
    # Día 03/03: reparar cambio de escenas + problema JSON datos mando (8:30-14:30, 6h)
    ("2026-03-03","08:30","14:30", [
        "R03F01T02 – Implementar envío de eventos vía socket",
        "R03F02T02 – Enviar datos procesados al servidor",
        "R03F02T02P01 – Verificar movimiento reflejado en personaje del juego",
    ]),
    # Día 04/03: mando virtual funcional + inputs JSON refactorizado + imágenes personajes Firebase (8:30-14:30, 6h)
    ("2026-03-04","08:30","14:30", [
        "R03F01T01 – Diseñar interfaz de mando virtual",
        "R03F01T01P01 – Presionar botón virtual y verificar acción en Unity",
        "R03F02T01 – Integrar API de sensores en MAUI Unity Mobile",
        "R03F02T01P01 – Mostrar valores en consola al interactuar con el sensor",
    ]),
    # Día 05/03: planificación minijuegos + bug desconexión + inicio mapeo monedas (8:30-14:30, 6h)
    ("2026-03-05","08:30","14:30", [
        "R02F01T02 – Generar código para acceso rápido",
        "R02F01T02P01 – Verificar que dos lobbies no compartan el mismo código",
    ]),
    # Día 06/03: minijuego monedas - mapeo entorno + scripts base (8:30-14:30, 6h)
    ("2026-03-06","08:30","14:30", [
        "R04F02T01 – Diseñar lógica de juego controlada por botones virtuales",
        "R04F02T01P01 – Ejecutar partida y validar inputs simultáneos",
    ]),
    # Día 09/03: monedas mejoras scripts + música IA + mapeo mapa + ajuste servidor (8:30-14:30, 6h)
    ("2026-03-09","08:30","14:30", [
        "R04F02T02 – Implementar temporizador de ronda",
        "R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo",
    ]),
    # Día 10/03: username Firebase sync + feedback UX + decoración lobby + HUD textos (8:30-14:30, 6h)
    ("2026-03-10","08:30","14:30", [
        "R04F01T02 – Implementar sistema de puntuación",
        "R04F01T02P01 – Verificar actualización de puntos en tiempo real",
        "R06F01T01 – Diseñar HUD con nombre avatar y puntuación",
        "R06F01T01P01 – Conectar jugadores y verificar visualización simultánea",
    ]),
    # Día 11/03: movimiento mejorado + audio + tiers monedas + count down audio (8:30-14:30, 6h)
    ("2026-03-11","08:30","14:30", [
        "R04F02T01 – Diseñar lógica de juego controlada por botones virtuales",
        "R04F02T01P01 – Ejecutar partida y validar inputs simultáneos",
        "R04F01T01 – Diseñar mecánica basada en acciones generadas por el uso de un sensor",
        "R04F01T01P01 – Simular partida completa con 2 jugadores",
    ]),
    # Día 12/03: fix spawn monedas imposibles + review siguiente minijuego (8:30-9:00, 30min)
    ("2026-03-12","08:30","09:00", [
        "R04F02T02 – Implementar temporizador de ronda",
        "R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo",
    ]),
    # Día 13/03: sin registro de actividad específica (entrada vacía en PDF)
    # Día 16/03: planificación siguiente minijuego football/bomba (15:26-16:00, 34min)
    ("2026-03-16","15:26","16:00", [
        "R04F01T01 – Diseñar mecánica basada en acciones generadas por el uso de un sensor",
        "R04F01T01P01 – Simular partida completa con 2 jugadores",
    ]),
]

# ─── READ ORIGINAL DURATIONS FROM XML ─────────────────────────────────────────
filename     = "ChirbitsplanORGININAL.xml"
out_filename = "ChirbitsplanORGININAL_updated.xml"

tree = ET.parse(filename)
root = tree.getroot()
ns   = {"ns": "http://schemas.microsoft.com/project"}

original = {}  # name -> {orig_h, orig_start, orig_finish}
for t in root.findall('.//ns:Task', ns):
    name_el   = t.find('ns:Name', ns)
    dur_el    = t.find('ns:Duration', ns)
    start_el  = t.find('ns:Start', ns)
    finish_el = t.find('ns:Finish', ns)
    if name_el is not None and dur_el is not None:
        original[name_el.text] = {
            "orig_h":      parse_dur(dur_el.text),
            "orig_start":  datetime.strptime(start_el.text[:19],  "%Y-%m-%dT%H:%M:%S"),
            "orig_finish": datetime.strptime(finish_el.text[:19], "%Y-%m-%dT%H:%M:%S"),
        }

# ─── ACCUMULATE DIARY HOURS ────────────────────────────────────────────────────
diary_acc = {}  # name -> {first_diary_dt, last_diary_dt, diary_h}
for date, start, end, tasks in sessions:
    dt_s  = datetime.strptime(f"{date} {start}", "%Y-%m-%d %H:%M")
    dt_e  = datetime.strptime(f"{date} {end}",   "%Y-%m-%d %H:%M")
    dur_h = (dt_e - dt_s).total_seconds() / 3600.0
    per_h = dur_h / len(tasks)
    for t in tasks:
        if t in diary_acc:
            e = diary_acc[t]
            e['first_dt'] = min(e['first_dt'], dt_s)
            e['last_dt']  = max(e['last_dt'],  dt_e)
            e['diary_h'] += per_h
        else:
            diary_acc[t] = {'first_dt': dt_s, 'last_dt': dt_e, 'diary_h': per_h}

# ─── BUILD FINAL PATCHES ──────────────────────────────────────────────────────
# Start    = diary first session start (real date from PDF)
# Finish   = diary LAST session end   (real date from PDF)
# Duration = original planned hours + all diary actual hours
patches = {}
for name, d in diary_acc.items():
    orig   = original.get(name)
    orig_h = orig['orig_h'] if orig else 0.0

    total_h    = orig_h + d['diary_h']
    new_start  = d['first_dt']   # earliest diary session start
    new_finish = d['last_dt']    # LAST diary session end (the real plazo)

    patches[name] = {
        "start":    fmt_dt(new_start),
        "finish":   fmt_dt(new_finish),
        "duration": fmt_dur(total_h),
        "resume":   fmt_dt(new_start),
    }

# ─── APPLY ────────────────────────────────────────────────────────────────────
with open(filename, "r", encoding="utf-8") as f:
    content = f.read()

ok = miss = 0
for name, patch in patches.items():
    content, hit = apply_patch(content, name, patch)
    if hit:
        ok += 1
        print(f"  [OK] {name[:60]}  {patch['start'][:10]} → {patch['finish'][:10]}  {patch['duration']}")
    else:
        miss += 1

with open(out_filename, "w", encoding="utf-8") as f:
    f.write(content)

print(f"\nDone: {ok} updated, {miss} missed → {out_filename}")
