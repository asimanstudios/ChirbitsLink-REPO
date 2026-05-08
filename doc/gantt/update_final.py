"""
Final additive Gantt updater with ROLL-UP logic for Parent Tasks.
- Reads existing Duration/Start/Finish from ChirbitsplanORGININAL.xml
- ADDS diary hours to the planned hours (additive).
- Applies roll-ups so that parent Summary tasks reflect the correct start/end dates.
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
    for field in ["Start", "Finish", "Resume"]:
        if field in patch:
            block = re.sub(rf'<{field}>[^<]*</{field}>', f'<{field}>{patch[field]}</{field}>', block)
    if "duration" in patch:
        block = re.sub(r'<Duration>[^<]*</Duration>', f'<Duration>{patch["duration"]}</Duration>', block)
        block = re.sub(r'(<ExtendedAttribute>\s*<FieldID>188743783</FieldID>\s*<Value>)[^<]*(</Value>)', rf'\g<1>{patch["duration"]}\g<2>', block)
    block = re.sub(r'<RemainingDuration>[^<]*</RemainingDuration>', '<RemainingDuration>PT0H0M0S</RemainingDuration>', block)
    block = re.sub(r'<PercentComplete>[^<]*</PercentComplete>', '<PercentComplete>100</PercentComplete>', block)
    return block

def apply_patch(content, needle_value, patch, field="Name"):
    needle = f'<{field}>{needle_value}</{field}>'
    search_pos = 0
    while True:
        open_tag = content.find('<Task>', search_pos)
        if open_tag == -1: return content, False
        close_tag = content.find('</Task>', open_tag) + len('</Task>')
        block = content[open_tag:close_tag]
        if needle in block:
            content = content[:open_tag] + patch_block(block, patch) + content[close_tag:]
            return content, True
        search_pos = close_tag

# ─── DIARY SESSIONS (from TFG CHIRBITS (2).pdf) ──────────────────────────────
sessions = [
    ("2026-02-17","08:30","14:30", ["R07F04T01P01 – Verificar que cumpla con el formato solicitado"]),
    ("2026-02-18","08:30","14:30", ["R01F01T03P01 – Visualizar pantalla de registro y validar campos obligatorios"]),
    ("2026-02-19","08:30","11:30", ["R01F01T02P01 – Enviar solicitud POST desde app móvil y verificar respuesta exitosa"]),
    ("2026-02-19","11:30","14:30", ["R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador"]),
    ("2026-02-20","08:30","14:30", ["R02F01T01P01 – Crear lobby y obtener código único de acceso"]),
    ("2026-02-24","08:30","11:00", ["R07F01T01P01 – Validar coherencia del flujo de datos", "R07F03T01P01 – Verificar que sea coherente con las entidades que requiere el sistema"]),
    ("2026-02-25","08:30","14:30", ["R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador"]),
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

    # --- NEW APRIL SESSIONS FROM PDF (2) ---
    # April 5: Hook game design
    ("2026-04-05", "11:00", "15:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    # April 6-12: Hook game systems & bug fixes (Weekends 11-15)
    ("2026-04-06", "18:00", "22:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-04-07", "18:00", "22:00", ["R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo"]),
    ("2026-04-08", "18:00", "22:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-04-09", "18:00", "22:00", ["R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo"]),
    ("2026-04-10", "18:00", "22:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    ("2026-04-11", "11:00", "15:00", ["R04F02T02P01 – Finalizar ronda automáticamente al cumplirse el tiempo"]),
    ("2026-04-12", "11:00", "15:00", ["R04F02T01P01 – Ejecutar partida y validar inputs simultáneos"]),
    # April 13-20: Interconnection, Lobby, Accounts, Mobile app, Stats, Network (Weekends 11-15)
    ("2026-04-13", "18:00", "22:00", ["R02F03T01P01 – Verificar actualización en pantalla principal al conectarse un jugador"]),
    ("2026-04-14", "18:00", "22:00", ["R02F01T01P01 – Crear lobby y obtener código único de acceso"]),
    ("2026-04-15", "18:00", "22:00", ["R02F02T01P01 – Introducir código válido y verificar conexión al lobby"]),
    ("2026-04-16", "18:00", "22:00", ["R03F01T01P01 – Presionar botón virtual y verificar acción en Unity"]),
    ("2026-04-17", "18:00", "22:00", ["R05F01T01P01 – Registrar puntuación al finalizar partida y validar cambios"]),
    ("2026-04-18", "11:00", "15:00", ["R07F04T04 – Corregir errores"]),
    ("2026-04-19", "11:00", "15:00", ["R07F04T04 – Corregir errores"]),
    ("2026-04-20", "18:00", "22:00", ["R02F03T01 – Definir sistema de conexión a utilizar sockets WebSockets o Bluetooth"]),
    
    # --- NEW LATE-APRIL & MAY SESSIONS FROM PDF (3) ---
    ("2026-04-21", "18:00", "22:00", ["R06F01T01P01 – Conectar jugadores y verificar visualización simultánea"]),
    ("2026-04-22", "18:00", "22:00", ["R02F03T01 – Definir sistema de conexión a utilizar sockets WebSockets o Bluetooth"]),
    ("2026-04-23", "18:00", "22:00", ["R05F01T01P01 – Registrar puntuación al finalizar partida y validar cambios"]),
    ("2026-04-24", "18:00", "22:00", ["R07F04T04 – Corregir errores"]),
    ("2026-04-25", "11:00", "15:00", ["R02F03T01 – Definir sistema de conexión a utilizar sockets WebSockets o Bluetooth"]),
    ("2026-04-26", "11:00", "15:00", ["R05F01T01P01 – Registrar puntuación al finalizar partida y validar cambios"]),
    ("2026-04-27", "18:00", "22:00", ["R07F04T04 – Corregir errores"]),
    ("2026-04-28", "18:00", "22:00", ["R03F01T01P01 – Presionar botón virtual y verificar acción en Unity"]),
    ("2026-04-29", "18:00", "22:00", ["R06F02T01P01 – Modificar puntuación y comprobar actualización inmediata"]),
    ("2026-04-30", "18:00", "22:00", ["R07F04T04 – Corregir errores"]),
    
    # May (Documentación y memoria)
    ("2026-05-01", "08:00", "12:00", ["R07F04T02 – Cumplimentar diferentes secciones de la memoria"]),
    ("2026-05-02", "14:00", "18:00", ["R07F04T04 – Corregir errores"]),
    ("2026-05-03", "19:00", "23:00", ["R02F03T01 – Definir sistema de conexión a utilizar sockets WebSockets o Bluetooth"]),
    ("2026-05-04", "09:00", "13:00", ["R05F01T01P01 – Registrar puntuación al finalizar partida y validar cambios"]),
    ("2026-05-05", "08:00", "14:30", ["R07F04T04 – Corregir errores"]),
    ("2026-05-05", "14:30", "18:00", ["R06F01T01P01 – Conectar jugadores y verificar visualización simultánea"]),
    ("2026-05-06", "19:00", "21:30", ["R07F04T04 – Corregir errores"]),
    ("2026-05-07", "20:00", "22:30", ["R07F04T02 – Cumplimentar diferentes secciones de la memoria"]),
]

milestones = [
    ("Hito Entrega 1", "2026-03-18T09:00:00"),
    ("Hito Entrega 2", "2026-04-07T09:00:00"),
    ("Hito Entrega 3", "2026-04-21T09:00:00"),
    ("Hito Entrega 4", "2026-05-05T09:00:00"),
    ("Hito Entrega 5", "2026-05-19T09:00:00"),
]

# ─── READ XML ────────────────────────────────────────────────────────────────
filename     = "ChirbitsplanORGININAL.xml"
out_filename = "ChirbitsplanORGININAL_updated.xml"

tree = ET.parse(filename)
root = tree.getroot()
ns   = {"ns": "http://schemas.microsoft.com/project"}

original = {}
for t in root.findall('.//ns:Task', ns):
    uid = t.find('ns:UID', ns).text
    name_el = t.find('ns:Name', ns)
    dur_el = t.find('ns:Duration', ns)
    start_el = t.find('ns:Start', ns)
    finish_el = t.find('ns:Finish', ns)
    if name_el is not None and dur_el is not None:
        original[name_el.text] = {
            "uid": uid,
            "orig_h": parse_dur(dur_el.text),
            "orig_start": datetime.strptime(start_el.text[:19], "%Y-%m-%dT%H:%M:%S"),
            "orig_finish": datetime.strptime(finish_el.text[:19], "%Y-%m-%dT%H:%M:%S"),
        }

# ─── ACCUMULATE DIARY ────────────────────────────────────────────────────────
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

# ─── BUILD PATCHES FOR LEAVES ─────────────────────────────────────────────────
patches = {}
final_dates = {} # To store the final dates for roll-up calculation
for name, d in diary_acc.items():
    orig   = original.get(name)
    orig_h = orig['orig_h'] if orig else 0.0
    total_h    = orig_h + d['diary_h']
    s_dt, f_dt = d['first_dt'], d['last_dt']
    patches[name] = {
        "start":    fmt_dt(s_dt),
        "finish":   fmt_dt(f_dt),
        "duration": fmt_dur(total_h),
        "resume":   fmt_dt(s_dt),
    }
    if orig:
        final_dates[orig["uid"]] = (s_dt, f_dt)

# ─── CALCULATE ROLL-UPS FOR SUMMARIES ─────────────────────────────────────────
# Read hierarchy to map summaries to children
task_list = []
for t in root.findall('.//ns:Task', ns):
    uid = t.find('ns:UID', ns).text
    lvl = int(t.find('ns:OutlineLevel', ns).text)
    is_sum = t.find('ns:Summary', ns).text == '1'
    start_str = t.find('ns:Start', ns).text[:19]
    finish_str = t.find('ns:Finish', ns).text[:19]
    s_dt = datetime.strptime(start_str, "%Y-%m-%dT%H:%M:%S")
    f_dt = datetime.strptime(finish_str, "%Y-%m-%dT%H:%M:%S")
    if uid not in final_dates:
        final_dates[uid] = (s_dt, f_dt) # default
    task_list.append({'uid': uid, 'lvl': lvl, 'is_sum': is_sum})

summary_patches = {}
for i, task in enumerate(task_list):
    if task['is_sum']:
        # Find all direct or indirect children (any consecutive task with higher outline level)
        children_uids = []
        for child in task_list[i+1:]:
            if child['lvl'] <= task['lvl']:
                break
            if not child['is_sum']:
                children_uids.append(child['uid'])
        
        # Calculate min start and max finish
        if children_uids:
            min_s = min(final_dates[c][0] for c in children_uids if c in final_dates)
            max_f = max(final_dates[c][1] for c in children_uids if c in final_dates)
            summary_patches[task['uid']] = {
                "Start": fmt_dt(min_s),
                "Finish": fmt_dt(max_f),
                "Resume": fmt_dt(min_s)
            }

# ─── APPLY EXISTING UPDATES ──────────────────────────────────────────────────
with open(filename, "r", encoding="utf-8") as f:
    content = f.read()

# Apply Leaf Patches
ok = miss = 0
for name, patch in patches.items():
    content, hit = apply_patch(content, name, patch, field="Name")
    if hit: ok += 1
    else: miss += 1

# Apply Summary Roll-ups
for uid_sum, patch in summary_patches.items():
    content, hit = apply_patch(content, uid_sum, patch, field="UID")

# ─── INSERT OR UPDATE MILESTONES ──────────────────────────────────────────────
# We only insert milestones if they aren't already there (to avoid duplicates)
for m_name, m_date in milestones:
    if f'<Name>{m_name}</Name>' not in content:
        tasks_section_end = content.rfind('</Tasks>')
        uid = str(int(task_list[-1]['uid']) + 1)
        milestone_xml = f"""
            <Task>
                <UID>{uid}</UID>
                <ID>{uid}</ID>
                <Name>{m_name}</Name>
                <Type>0</Type>
                <IsNull>0</IsNull>
                <CreateDate>2026-03-17T14:20:00</CreateDate>
                <WBS>{uid}</WBS>
                <OutlineNumber>{uid}</OutlineNumber>
                <OutlineLevel>1</OutlineLevel>
                <Priority>500</Priority>
                <Start>{m_date}</Start>
                <Finish>{m_date}</Finish>
                <Duration>PT0H0M0S</Duration>
                <DurationFormat>39</DurationFormat>
                <Resume>{m_date}</Resume>
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
                <ConstraintDate>{m_date}</ConstraintDate>
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
        content = content[:tasks_section_end] + milestone_xml + content[tasks_section_end:]
        task_list.append({'uid': uid, 'lvl': 1, 'is_sum': False})

with open(out_filename, "w", encoding="utf-8") as f:
    f.write(content)

print(f"\nDone: {ok} updated, {len(summary_patches)} summaries rolled up -> {out_filename}")
