"""
Final additive Gantt updater – maps diary sessions to the correct LEAF tasks.
- Reads existing Duration/Start/Finish from ChirbitsplanORGININAL.xml
- ADDS diary hours to the planned hours (additive).
- Inserts new delivery milestones (UID 83+).
"""
import xml.etree.ElementTree as ET
from datetime import datetime, timedelta
import re

def parse_dur(s):
    m = re.match(r'PT(\d+)H(\d+)M', s)
    if m: return int(m.group(1)) + int(m.group(2)) / 60.0
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
    needle = f'<Name>{task_name}</Name>'
    pos = content.find(needle)
    if pos == -1: return content, False
    open_tag = content.rfind('<Task>', 0, pos)
    close_tag = content.find('</Task>', pos) + len('</Task>')
    block = content[open_tag:close_tag]
    content = content[:open_tag] + patch_block(block, patch) + content[close_tag:]
    return content, True

# ─── DIARY SESSIONS (from TFG CHIRBITS (1).pdf) ──────────────────────────────
sessions = [
    # Feb (Previously confirmed)
    ("2026-02-17","08:30","14:30", ["R07F04T01P01 – Verificar que cumpla con el formato solicitado"]),
    ("2026-02-18","08:30","14:30", ["R01F01T03P01 – Visualizar pantalla de registro y validar campos obligatorios"]),
    ("2026-02-19","08:30","11:30", ["R01F01T02P01 – Enviar solicitud POST desde app móvil y verificar respuesta exitosa"]),
    ("2026-02-19","11:30","14:30", ["R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador"]),
    ("2026-02-20","08:30","14:30", ["R02F01T01P01 – Crear lobby y obtener código único de acceso"]),
    ("2026-02-24","08:30","11:00", ["R07F01T01P01 – Validar coherencia del flujo de datos", "R07F03T01P01 – Verificar que sea coherente con las entidades que requiere el sistema"]),
    ("2026-02-25","08:30","14:30", ["R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador"]),
    
    # Mar
    ("2026-03-02","08:30","14:30", ["R02F02T01P01 – Introducir código válido y verificar conexión al lobby"]),
    ("2026-03-03","08:30","14:30", ["R03F02T02P01 – Verificar movimiento reflejado en personaje del juego"]),
    ("2026-03-04","08:30","14:30", ["R03F01T01P01 – Presionar botón virtual y verificar acción en Unity"]),
    ("2026-03-05","08:30","14:30", ["R02F01T02P01 – Verificar que dos lobbies no compartan el mismo código"]),
    ("2026-03-06","08:30","14:30", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-09","08:30","14:30", ["R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo"]),
    ("2026-03-10","08:30","14:30", ["R04F01T02P01 – Verificar actualización de puntos en tiempo real"]),
    ("2026-03-11","08:30","14:30", ["R04F01T01P01 – Simular partida completa con 2 jugadores"]),
    ("2026-03-12","08:30","09:00", ["R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo"]),
    ("2026-03-16","15:26","16:00", ["R04F01T01P01 – Simular partida completa con 2 jugadores"]),
    
    # --- NEW FROM PDF (1) ---
    ("2026-03-17","13:00","15:34", ["R07F04T01P01 – Verificar que cumpla con el formato solicitado"]),
    ("2026-03-19","08:30","16:30", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-20","08:30","14:30", ["R04F01T02P01 – Verificar actualización de puntos en tiempo real", "R05F01T01P01 – Registrar puntuación al finalizar partida y validar cambios"]),
    ("2026-03-23","15:00","17:08", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-24","14:00","15:39", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-25","13:40","15:45", ["R07F01T01P01 – Validar coherencia del flujo de datos", "R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-26","15:20","17:17", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-03-30","08:30","14:30", ["R07F04T04 – Corregir errores"]),
    ("2026-04-01","16:00","18:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-04-02","17:00","20:35", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-04-03","14:27","18:30", ["R07F04T04 – Corregir errores"]),
]

milestones = [
    ("Hito Entrega 1", "2026-03-18T09:00:00"),
    ("Hito Entrega 2", "2026-04-07T09:00:00"),
    ("Hito Entrega 3", "2026-04-21T09:00:00"),
    ("Hito Entrega 4", "2026-05-05T09:00:00"),
    ("Hito Entrega 5", "2026-05-19T09:00:00"),
]

# ─── READ ORIGINAL XML ────────────────────────────────────────────────────────
filename     = "ChirbitsplanORGININAL.xml"
out_filename = "ChirbitsplanORGININAL_updated.xml"

tree = ET.parse(filename)
root = tree.getroot()
ns   = {"ns": "http://schemas.microsoft.com/project"}

original = {}
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

# ─── ACCUMULATE DIARY ──────────────────────────────────────────────────────────
diary_acc = {}
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

# ─── BUILD PATCHES ───────────────────────────────────────────────────────────
patches = {}
for name, d in diary_acc.items():
    orig   = original.get(name)
    orig_h = orig['orig_h'] if orig else 0.0
    total_h    = orig_h + d['diary_h']
    patches[name] = {
        "start":    fmt_dt(d['first_dt']),
        "finish":   fmt_dt(d['last_dt']),
        "duration": fmt_dur(total_h),
        "resume":   fmt_dt(d['first_dt']),
    }

# ─── APPLY EXISTING UPDATES ──────────────────────────────────────────────────
with open(filename, "r", encoding="utf-8") as f:
    content = f.read()

ok = miss = 0
for name, patch in patches.items():
    content, hit = apply_patch(content, name, patch)
    if hit: ok += 1
    else: miss += 1

# ─── INSERT MILESTONES ────────────────────────────────────────────────────────
tasks_section_end = content.rfind('</Tasks>')
milestone_xml = ""
uid = 83
for name, date_str in milestones:
    milestone_xml += f"""
        <Task>
            <UID>{uid}</UID>
            <ID>{uid}</ID>
            <Name>{name}</Name>
            <Type>0</Type>
            <IsNull>0</IsNull>
            <CreateDate>2026-03-17T14:20:00</CreateDate>
            <WBS>{uid}</WBS>
            <OutlineNumber>{uid}</OutlineNumber>
            <OutlineLevel>1</OutlineLevel>
            <Priority>500</Priority>
            <Start>{date_str}</Start>
            <Finish>{date_str}</Finish>
            <Duration>PT0H0M0S</Duration>
            <DurationFormat>39</DurationFormat>
            <Resume>{date_str}</Resume>
            <ResumeValid>0</ResumeValid>
            <EffortDriven>1</EffortDriven>
            <Recurring>0</Recurring>
            <OverAllocated>0</OverAllocated>
            <Estimated>0</Estimated>
            <Milestone>1</Milestone>
            <Summary>0</Summary>
            <Critical>0</Critical>
            <IsSubproject>0</IsSubproject>
            <IsSubprojectReadOnly>0</IsSubprojectReadOnly>
            <ExternalTask>0</ExternalTask>
            <FixedCostAccrual>2</FixedCostAccrual>
            <PercentComplete>0</PercentComplete>
            <PercentWorkComplete>0</PercentWorkComplete>
            <RemainingDuration>PT0H0M0S</RemainingDuration>
            <ConstraintType>4</ConstraintType>
            <CalendarUID>-1</CalendarUID>
            <ConstraintDate>{date_str}</ConstraintDate>
            <LevelAssignments>0</LevelAssignments>
            <LevelingCanSplit>0</LevelingCanSplit>
            <LevelingDelay>0</LevelingDelay>
            <LevelingDelayFormat>7</LevelingDelayFormat>
            <IgnoreResourceCalendar>0</IgnoreResourceCalendar>
            <HideBar>0</HideBar>
            <Rollup>0</Rollup>
            <EarnedValueMethod>0</EarnedValueMethod>
            <Active>1</Active>
            <Manual>0</Manual>
        </Task>"""
    uid += 1

content = content[:tasks_section_end] + milestone_xml + content[tasks_section_end:]

with open(out_filename, "w", encoding="utf-8") as f:
    f.write(content)

print(f"\nDone: {ok} updated, {len(milestones)} milestones inserted → {out_filename}")
