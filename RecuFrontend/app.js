const API_BASE = '__API_BASE__';

const state = {
  token: localStorage.getItem('token') || '',
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  view: 'campaigns',
  campaignId: null,
  campaign: null,
  characterId: null,
  character: null,
  campaignFilters: {},
  characterFilters: {},
  publicCampaignId: null,
};

const $ = (id) => document.getElementById(id);

function show(el, visible) {
  if (!el) return;
  el.classList.toggle('hidden', !visible);
}

function setText(el, text) {
  if (!el) return;
  el.textContent = text ?? '';
}

function setHtml(el, html) {
  if (!el) return;
  el.innerHTML = html ?? '';
}

let toastTimer = null;
function toast(message) {
  const el = $('toast');
  if (!el) return;
  setText(el, message);
  show(el, true);
  if (toastTimer) clearTimeout(toastTimer);
  toastTimer = setTimeout(() => show(el, false), 2800);
}

function unwrap(data) {
  return data?.value ?? data ?? [];
}

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  if (!(options.body instanceof FormData)) headers['Content-Type'] = 'application/json';

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error(err.message || `Error ${res.status}`);
  }
  if (res.status === 204) return null;
  const ct = res.headers.get('content-type') || '';
  if (ct.includes('application/json')) return res.json();
  return res.blob();
}

async function apiBlob(path) {
  const headers = {};
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  const res = await fetch(`${API_BASE}${path}`, { headers });
  if (!res.ok) throw new Error('No se pudo descargar el archivo');
  return res.blob();
}

function buildQuery(params) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') q.set(k, v);
  });
  const s = q.toString();
  return s ? `?${s}` : '';
}

// ─── Auth & shell ───────────────────────────────────────────

const ROLE_LABELS = {
  Admin: 'Administrador',
  Master: 'Dungeon Master',
  User: 'Jugador',
};

function isAdmin() {
  return state.user?.isAdmin === true || state.user?.role === 'Admin';
}

function isMaster() {
  return state.user?.isMaster === true || state.user?.role === 'Master';
}

function isPlayer() {
  return state.user && !isAdmin() && !isMaster();
}

function canManageGame() {
  return isMaster();
}

function canManageOwnCharacters() {
  return isMaster() || isPlayer();
}

function roleLabel(role) {
  return ROLE_LABELS[role] || role || '—';
}

function roleBadgeHtml() {
  if (!state.user) return '';
  const role = state.user.role;
  const label = roleLabel(role);
  const cls = isAdmin() ? 'badge--admin' : isMaster() ? 'badge--master' : 'badge--inactive';
  return `<span class="badge ${cls}">${esc(label)}</span>`;
}

function applyRolePermissions() {
  const master = canManageGame();
  const player = isPlayer();
  const chars = canManageOwnCharacters();
  show($('campaign-create-section'), master);
  show($('btn-edit-campaign'), master);
  show($('btn-delete-campaign'), master);
  show($('player-explore-section'), player);
  show($('character-form'), chars);
  show($('btn-edit-character'), chars);
  show($('btn-delete-character'), chars);
  setText($('campaign-list-title'), player ? '📋 Campañas con mis personajes' : '🔍 Mis campañas');
  const empty = $('empty-state');
  if (empty) {
    const h2 = empty.querySelector('h2');
    const p = empty.querySelector('p');
    if (h2) setText(h2, player ? 'Únete a una campaña pública' : 'Elige una campaña');
    if (p) {
      setText(p, player
        ? 'Explora las campañas públicas de la izquierda y crea tu personaje para unirte.'
        : 'Selecciona una de la lista o crea una nueva para empezar tu aventura.');
    }
  }
  document.querySelectorAll('#character-form [name="isNpc"], #character-edit-form [name="isNpc"]').forEach((el) => {
    const label = el.closest('.checkbox');
    if (label) {
      show(label, master);
      if (!master) el.checked = false;
    }
  });
  document.querySelectorAll('#character-filters [name="isNpc"]').forEach((el) => {
    const label = el.closest('.checkbox');
    if (label) {
      show(label, master);
      if (!master) el.checked = false;
    }
  });
  if (player) loadExploreCampaigns();
}

function showApp(loggedIn) {
  show($('auth-section'), !loggedIn);
  show($('app-section'), loggedIn);
  show($('btn-logout'), loggedIn);
  show($('main-nav'), loggedIn);
  if (loggedIn && state.user) {
    setHtml($('user-info'), `<strong>${esc(state.user.username)}</strong> ${roleBadgeHtml()}`);
    const admin = isAdmin();
    show($('nav-admin'), admin);
    if (admin) $('nav-admin').classList.remove('hidden');
    else $('nav-admin')?.classList.add('hidden');
    applyRolePermissions();
    switchView(state.view || 'campaigns');
    if (state.view === 'campaigns' || !state.view) loadCampaigns();
  }
}

function readSessionRole(form) {
  const selected = form.querySelector('input[name="sessionRole"]:checked');
  return selected?.value || 'User';
}

async function login(username, password, sessionRole) {
  const data = await api('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password, sessionRole }),
  });
  saveSession(data);
}

async function register(username, password, displayName, sessionRole) {
  const data = await api('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, password, displayName, sessionRole }),
  });
  saveSession(data);
}

function saveSession(data) {
  state.token = data.accessToken;
  state.user = {
    username: data.username,
    role: data.role,
    userId: data.userId,
    isAdmin: data.isAdmin ?? data.role === 'Admin',
    isMaster: data.isMaster ?? data.role === 'Master',
  };
  localStorage.setItem('token', state.token);
  localStorage.setItem('user', JSON.stringify(state.user));
  showApp(true);
}

async function refreshSession() {
  if (!state.token) return;
  try {
    const me = await api('/api/auth/me');
    state.user = {
      ...state.user,
      role: me.role,
      isAdmin: me.isAdmin,
      isMaster: me.isMaster,
    };
    localStorage.setItem('user', JSON.stringify(state.user));
    showApp(true);
  } catch {
    logout();
  }
}

function logout() {
  state.token = '';
  state.user = null;
  state.campaignId = null;
  state.campaign = null;
  state.characterId = null;
  state.character = null;
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  showApp(false);
}

function switchView(view) {
  state.view = view;
  document.querySelectorAll('.nav-btn').forEach((b) => {
    b.classList.toggle('active', b.dataset.view === view);
  });
  document.querySelectorAll('.view').forEach((v) => v.classList.add('hidden'));
  const el = $(`view-${view}`);
  if (el) el.classList.remove('hidden');

  if (view === 'campaigns') {
    loadCampaigns();
    if (isPlayer()) loadExploreCampaigns();
  }
  if (view === 'public') loadPublicCampaigns();
  if (view === 'admin' && isAdmin()) loadAdmin();
}

// ─── Modal ──────────────────────────────────────────────────

let modalResolve = null;

function confirmModal(title, body) {
  return new Promise((resolve) => {
    modalResolve = resolve;
    setText($('modal-title'), title);
    setText($('modal-body'), body);
    show($('modal'), true);
  });
}

function closeModal(result) {
  show($('modal'), false);
  if (modalResolve) modalResolve(result);
  modalResolve = null;
}

// ─── Campaigns ──────────────────────────────────────────────

async function loadExploreCampaigns() {
  if (!isPlayer()) return;
  const list = $('explore-list');
  const loading = $('explore-loading');
  if (!list) return;
  show(loading, true);
  list.innerHTML = '';
  try {
    const campaigns = unwrap(await api('/api/campaigns/explore?sortBy=name&sortDir=asc'));
    if (!campaigns.length) {
      list.innerHTML = '<li class="empty-item">No hay campañas públicas activas.</li>';
      return;
    }
    campaigns.forEach((c) => {
      const li = document.createElement('li');
      li.innerHTML = `
        <div style="display:flex;align-items:center;justify-content:space-between;gap:.5rem">
          <div class="entity-card__title">${esc(c.name)}</div>
          <span class="badge badge--public">Unirse</span>
        </div>
        <div class="entity-card__meta">${esc(c.setting)}</div>
      `;
      li.onclick = () => openCampaign(c.id);
      list.appendChild(li);
    });
  } catch (e) {
    list.innerHTML = `<li class="empty-item">${esc(e.message)}</li>`;
  } finally {
    show(loading, false);
  }
}

async function loadCampaigns() {
  const list = $('campaign-list');
  const loading = $('campaigns-loading');
  const error = $('campaigns-error');
  show(error, false);
  show(loading, true);
  list.innerHTML = '';

  const f = state.campaignFilters;
  const query = buildQuery({
    search: f.search,
    setting: f.setting,
    isActive: f.isActive === true ? true : undefined,
    isPublic: f.isPublic === true ? true : undefined,
    sortBy: f.sortBy || 'name',
    sortDir: f.sortDir || 'asc',
  });

  try {
    const campaigns = unwrap(await api(`/api/campaigns${query}`));
    if (!campaigns.length) {
      list.innerHTML = isPlayer()
        ? '<li class="empty-item">Aún no tienes personajes en ninguna campaña.</li>'
        : '<li class="empty-item">No hay campañas. Crea la primera arriba.</li>';
      return;
    }
    campaigns.forEach((c) => {
      const li = document.createElement('li');
      li.className = state.campaignId === c.id ? 'selected' : '';
      li.innerHTML = `
        <div style="display:flex;align-items:center;justify-content:space-between;gap:.5rem">
          <div class="entity-card__title">${esc(c.name)}</div>
          <div style="display:flex;gap:.3rem;flex-shrink:0">
            ${c.isPublic ? '<span class="badge badge--public">Pública</span>' : ''}
            <span class="badge ${c.isActive ? 'badge--active' : 'badge--inactive'}">${c.isActive ? '●' : '○'}</span>
          </div>
        </div>
        <div class="entity-card__meta" style="margin-top:.25rem">${esc(c.setting)}</div>
      `;
      li.onclick = () => openCampaign(c.id);
      list.appendChild(li);
    });
  } catch (e) {
    setText(error, e.message);
    show(error, true);
  } finally {
    show(loading, false);
  }
}

async function openCampaign(id) {
  state.campaignId = id;
  state.characterId = null;
  state.character = null;
  show($('empty-state'), false);
  show($('character-detail'), false);
  show($('campaign-detail'), true);
  show($('character-filters'), isMaster());
  show($('campaign-edit-form'), false);

  try {
    const c = await api(`/api/campaigns/${id}`);
    state.campaign = c;
    renderCampaignDetail(c);
    applyRolePermissions();
    show($('player-join-hint'), isPlayer() && c.isPublic && c.isActive);
    if (isPlayer()) {
      const details = $('character-form')?.querySelector('details');
      if (details) details.open = true;
    }
    loadCharacters();
    loadCampaigns();
    if (isPlayer()) loadExploreCampaigns();
  } catch (e) {
    toast(e.message);
    state.campaignId = null;
    state.campaign = null;
    show($('campaign-detail'), false);
    show($('player-join-hint'), false);
    show($('empty-state'), true);
  }
}

function renderCampaignDetail(c) {
  setText($('campaign-title'), c.name);
  setText($('campaign-desc'), c.description || 'Sin descripción.');
  const badges = $('campaign-badges');
  badges.innerHTML = `
    ${c.isPublic ? '<span class="badge badge--public">Pública</span>' : ''}
    <span class="badge ${c.isActive ? 'badge--active' : 'badge--inactive'}">${c.isActive ? 'Activa' : 'Inactiva'}</span>
  `;
  fillForm($('campaign-edit-form'), {
    name: c.name,
    setting: c.setting,
    description: c.description || '',
    isPublic: c.isPublic,
    isActive: c.isActive,
  });
}

async function loadCharacters() {
  const list = $('character-list');
  const loading = $('characters-loading');
  const error = $('characters-error');
  show(error, false);
  show(loading, true);
  list.innerHTML = '';

  const f = state.characterFilters;
  const query = buildQuery({
    name: f.name,
    race: f.race,
    characterClass: f.characterClass,
    isNpc: f.isNpc === true ? true : undefined,
    sortBy: 'name',
    sortDir: 'asc',
  });

  try {
    const chars = unwrap(await api(`/api/campaigns/${state.campaignId}/characters${query}`));
    if (!chars.length) {
      list.innerHTML = '<li class="empty-item">Aún no hay personajes en esta campaña.</li>';
      return;
    }
    chars.forEach((ch) => {
      const li = document.createElement('li');
      li.innerHTML = `
        <div style="display:flex;align-items:center;justify-content:space-between;gap:.5rem">
          <div class="entity-card__title">${esc(ch.name)}</div>
          <div style="display:flex;gap:.3rem;flex-shrink:0">
            ${ch.isNpc ? '<span class="badge badge--npc">NPC</span>' : ''}
            <span class="badge badge--inactive">Niv. ${ch.level}</span>
          </div>
        </div>
        <div class="entity-card__meta" style="margin-top:.25rem">
          ${esc(ch.race)} &nbsp;·&nbsp; ${esc(ch.characterClass)}
          &nbsp;&nbsp; <span style="opacity:.6">CA ${ch.armorClass} &nbsp; PG ${ch.hitPoints}</span>
        </div>
      `;
      li.onclick = () => openCharacter(ch.id);
      list.appendChild(li);
    });
  } catch (e) {
    setText(error, e.message);
    show(error, true);
  } finally {
    show(loading, false);
  }
}

async function openCharacter(id) {
  state.characterId = id;
  show($('campaign-detail'), false);
  show($('character-detail'), true);
  show($('character-edit-form'), false);

  try {
    const ch = normalizeCharacter(await api(`/api/campaigns/${state.campaignId}/characters/${id}`));
    state.character = ch;
    setText($('character-title'), ch.name);
    const meta = [ch.race, ch.characterClass, `Nivel ${ch.level}`];
    if (ch.alignment) meta.push(ch.alignment);
    if (ch.isNpc) meta.push('NPC');
    setText($('character-meta'), meta.join(' · '));
    renderSheet(ch);
    fillCharacterForm($('character-edit-form'), ch);
    switchCharTab('sheet');
    loadRollHistory();
    loadAttachments();
  } catch (e) {
    toast(e.message);
  }
}

function switchCharTab(tab) {
  document.querySelectorAll('[data-char-tab]').forEach((b) => {
    b.classList.toggle('active', b.dataset.charTab === tab);
  });
  show($('char-tab-sheet'), tab === 'sheet');
  show($('char-tab-dice'), tab === 'dice');
  show($('char-tab-files'), tab === 'files');
}

function statBlock(label, score) {
  const v = score ?? '—';
  const mod = score != null ? formatMod(abilityModValue(score)) : '—';
  return `
    <div class="stat">
      <div class="stat__abbr">${label}</div>
      <div class="stat__score">${v}</div>
      <div class="stat__mod">${mod}</div>
    </div>`;
}

function renderSkillsPanel(ch) {
  const rows = DND_SKILLS.map((skill) => {
    const mod = skillModifier(ch, skill.id);
    const proficient = isSkillProficient(ch, skill.id);
    const ab = DND_ABILITIES.find((a) => a.key === skill.ability);
    return `
      <button type="button" class="skill-row" data-skill-id="${skill.id}" data-skill-name="${escAttr(skill.name)}" title="Tirar ${esc(skill.name)} (1d20${formatMod(mod)})">
        <span class="skill-row__dot ${proficient ? 'skill-row__dot--on' : ''}" aria-hidden="true"></span>
        <span class="skill-row__name">${esc(skill.name)}</span>
        <span class="skill-row__ab">${ab?.label || ''}</span>
        <span class="skill-row__mod">${formatMod(mod)}</span>
      </button>`;
  }).join('');

  return `<div class="skills-panel"><div class="skills-panel__head">Competencias <span class="muted">(clic = tirada 1d20 + mod)</span></div><div class="skills-list">${rows}</div></div>`;
}

function renderSheet(raw) {
  const sheet = $('sheet');
  if (!sheet) return;
  const ch = normalizeCharacter(raw);
  const lore = [
    ch.background ? `<span><strong>Trasfondo:</strong> ${esc(ch.background)}</span>` : '',
    ch.alignment ? `<span><strong>Alineamiento:</strong> ${esc(ch.alignment)}</span>` : '',
    ch.languages ? `<span><strong>Idiomas:</strong> ${esc(ch.languages)}</span>` : '',
  ].filter(Boolean).join(' · ');

  sheet.innerHTML = `
    <div class="sheet__panel sheet__panel--hero">
      <div class="sheet__header">
        <h3 class="sheet__name">${esc(ch.name)}</h3>
        ${ch.isNpc ? '<span class="badge badge--npc">NPC</span>' : ''}
      </div>
      <div class="sheet__classline">${esc(ch.race)} · ${esc(ch.characterClass)} · Nivel ${ch.level}</div>
      ${lore ? `<div class="sheet__lore">${lore}</div>` : ''}
      <div class="kpi">
        <div class="kpi__item"><div class="kpi__label">CA</div><div class="kpi__value">${ch.armorClass}</div></div>
        <div class="kpi__item"><div class="kpi__label">Vida</div><div class="kpi__value">${ch.currentHitPoints}/${ch.hitPoints}</div></div>
        <div class="kpi__item"><div class="kpi__label">Comp.</div><div class="kpi__value">+${ch.proficiencyBonus}</div></div>
        <div class="kpi__item"><div class="kpi__label">Inic.</div><div class="kpi__value">${formatMod(abilityModValue(ch.dexterity))}</div></div>
      </div>
    </div>
    <div class="sheet__panel">
      <div class="sheet__section-title">Atributos y salvaciones</div>
      <div class="stats stats--6">
        ${DND_ABILITIES.map((a) => statBlock(a.label, ch[a.key])).join('')}
      </div>
      <p class="sheet__hint muted">Iniciativa = modificador de DES (${formatMod(abilityModValue(ch.dexterity))})</p>
    </div>
    <div class="sheet__panel sheet__panel--wide">
      ${renderSkillsPanel(ch)}
    </div>
  `;

  sheet.querySelectorAll('.skill-row').forEach((btn) => {
    btn.onclick = () => rollSkill(btn.dataset.skillId, btn.dataset.skillName);
  });
}

function fillCharacterForm(form, ch) {
  if (!form || !ch) return;
  const n = normalizeCharacter(ch);
  fillForm(form, n);
  const containerId = form.id === 'character-form' ? 'skill-profs-create' : 'skill-profs-edit';
  mountSkillProficiencies($(containerId), n.skillProficiencies);
}

async function rollSkill(skillId, skillName) {
  const ch = state.character;
  if (!ch || !state.characterId) return;
  const mod = skillModifier(ch, skillId);
  const expr = buildD20Expression(mod);
  const proficient = isSkillProficient(ch, skillId);
  const label = `${skillName} (${formatMod(mod)}${proficient ? ', competente' : ''})`;
  const mode = Number($('d20-mode')?.value || 0);

  try {
    const roll = await api(`/api/characters/${state.characterId}/rolls`, {
      method: 'POST',
      body: JSON.stringify({ label, diceExpression: expr, d20Mode: mode }),
    });
    switchCharTab('dice');
    if ($('dice-expression')) $('dice-expression').value = expr;
    showRollResult(roll);
    loadRollHistory();
    toast(`Tirada: ${skillName} ${formatMod(mod)}`);
  } catch (e) {
    toast(e.message);
  }
}

// ─── Rolls ────────────────────────────────────────────────────

async function doRoll() {
  const expr = $('dice-expression').value.trim();
  const mode = Number($('d20-mode').value);
  try {
    const roll = await api(`/api/characters/${state.characterId}/rolls`, {
      method: 'POST',
      body: JSON.stringify({ label: 'Tirada', diceExpression: expr, d20Mode: mode }),
    });
    showRollResult(roll);
    loadRollHistory();
  } catch (e) {
    toast(e.message);
  }
}

function showRollResult(roll) {
  const display = $('roll-display');
  const total = $('roll-total');
  const detail = $('roll-detail');
  display.classList.toggle('roll-display--crit', roll.isCritical);
  setText(total, roll.total);
  const modeLabel = roll.d20Mode === 1 ? ' · Ventaja' : roll.d20Mode === 2 ? ' · Desventaja' : '';
  setText(detail, `${roll.diceExpression} → [${roll.rawResults}]${modeLabel}${roll.isCritical ? ' · ¡CRÍTICO!' : ''}`);
  show(display, true);
}

async function loadRollHistory() {
  const list = $('roll-history');
  list.innerHTML = '';
  try {
    const rolls = unwrap(await api(`/api/characters/${state.characterId}/rolls`));
    if (!rolls.length) {
      list.innerHTML = '<li class="empty-item" style="cursor:default">Sin tiradas aún.</li>';
      return;
    }
    rolls.slice(0, 30).forEach((r) => {
      const li = document.createElement('li');
      const date = new Date(r.rolledAtUtc).toLocaleString('es');
      li.innerHTML = `
        <span>${esc(r.label || r.diceExpression)} <span class="muted">· ${date}</span></span>
        <span>
          <span class="timeline__total">${r.total}</span>
          ${r.isCritical ? '<span class="timeline__crit"> CRIT</span>' : ''}
        </span>
      `;
      list.appendChild(li);
    });
  } catch {
    list.innerHTML = '<li class="empty-item" style="cursor:default">No se pudo cargar el historial.</li>';
  }
}

// ─── Attachments ──────────────────────────────────────────────

async function loadAttachments() {
  const list = $('attachment-list');
  list.innerHTML = '';
  try {
    const items = unwrap(await api(`/api/characters/${state.characterId}/attachments`));
    if (!items.length) {
      list.innerHTML = '<li class="empty-item" style="cursor:default">Sin adjuntos.</li>';
      return;
    }
    for (const a of items) {
      const li = document.createElement('li');
      li.className = 'attachment-item';
      const size = formatBytes(a.sizeBytes);
      let thumb = '';
      if (a.isImage) {
        const imgSrc = a.secureUrl || null;
        if (imgSrc) {
          thumb = `<img class="attachment-thumb" src="${escAttr(imgSrc)}" alt="" />`;
        } else {
          try {
            const blob = await apiBlob(`/api/attachments/${a.id}`);
            const url = URL.createObjectURL(blob);
            thumb = `<img class="attachment-thumb" src="${url}" alt="" />`;
          } catch {
            thumb = '<div class="attachment-thumb">🖼</div>';
          }
        }
      } else {
        thumb = '<div class="attachment-thumb">📄</div>';
      }
      li.innerHTML = `
        ${thumb}
        <div class="attachment-info">
          <div class="attachment-name">${esc(a.fileName)}</div>
          <div class="attachment-size">${size} · ${esc(a.contentType || '')}</div>
        </div>
        <div class="btn-group">
          <button type="button" class="btn btn--ghost btn--sm" data-dl="${a.id}" data-name="${escAttr(a.fileName)}">Descargar</button>
          <button type="button" class="btn btn--danger btn--sm" data-del="${a.id}">Eliminar</button>
        </div>
      `;
      li.querySelector('[data-dl]')?.addEventListener('click', (e) => {
        e.stopPropagation();
        downloadFile(a.id, a.fileName, a.secureUrl);
      });
      li.querySelector('[data-del]')?.addEventListener('click', async (e) => {
        e.stopPropagation();
        const ok = await confirmModal('Eliminar adjunto', `¿Borrar "${a.fileName}"?`);
        if (!ok) return;
        try {
          await api(`/api/characters/${state.characterId}/attachments/${a.id}`, { method: 'DELETE' });
          toast('Adjunto eliminado');
          loadAttachments();
        } catch (err) {
          toast(err.message);
        }
      });
      list.appendChild(li);
    }
  } catch (e) {
    list.innerHTML = `<li class="empty-item" style="cursor:default">${esc(e.message)}</li>`;
  }
}

async function downloadFile(id, fileName, secureUrl) {
  try {
    if (secureUrl) {
      window.open(secureUrl, '_blank', 'noopener');
      return;
    }
    const blob = await apiBlob(`/api/attachments/${id}`);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  } catch (e) {
    toast(e.message);
  }
}

function formatBytes(n) {
  if (n < 1024) return `${n} B`;
  if (n < 1048576) return `${(n / 1024).toFixed(1)} KB`;
  return `${(n / 1048576).toFixed(1)} MB`;
}

// ─── Public campaigns ─────────────────────────────────────────

async function loadPublicCampaigns() {
  const grid = $('public-campaigns');
  const loading = $('public-loading');
  show($('public-characters-wrap'), false);
  show(grid, true);
  show(loading, true);
  grid.innerHTML = '';

  try {
    const campaigns = unwrap(await api('/api/public/campaigns?sortBy=name&sortDir=asc'));
    if (!campaigns.length) {
      grid.innerHTML = '<p class="muted">No hay campañas públicas en este momento.</p>';
      return;
    }
    campaigns.forEach((c) => {
      const card = document.createElement('div');
      card.className = 'public-card';
      const joinLabel = state.token && isPlayer() ? '<span class="badge badge--public">Unirse →</span>' : '';
      card.innerHTML = `
        <div style="display:flex;justify-content:space-between;align-items:flex-start;gap:.5rem">
          <h4 style="margin:0">${esc(c.name)}</h4>
          ${joinLabel}
        </div>
        <p>${esc(c.setting)}</p>
        <p class="muted" style="margin-top:0.5rem">${esc(c.description || 'Sin descripción')}</p>
      `;
      card.onclick = () => openPublicCampaign(c);
      grid.appendChild(card);
    });
  } catch (e) {
    grid.innerHTML = `<p class="notice notice--error">${esc(e.message)}</p>`;
  } finally {
    show(loading, false);
  }
}

async function openPublicCampaign(c) {
  if (state.token && isPlayer()) {
    switchView('campaigns');
    await openCampaign(c.id);
    toast('Crea tu personaje para unirte a la partida');
    return;
  }

  state.publicCampaignId = c.id;
  show($('public-campaigns'), false);
  show($('public-characters-wrap'), true);
  setText($('public-campaign-title'), `Personajes · ${c.name}`);
  const list = $('public-character-list');
  list.innerHTML = '';

  try {
    const chars = unwrap(await api(`/api/public/campaigns/${c.id}/characters?sortBy=name`));
    if (!chars.length) {
      list.innerHTML = '<li class="empty-item">Esta campaña no tiene personajes visibles.</li>';
      return;
    }
    chars.forEach((ch) => {
      const li = document.createElement('li');
      li.style.cursor = 'default';
      li.innerHTML = `
        <div class="entity-card__title">${esc(ch.name)}</div>
        <div class="entity-card__meta">${esc(ch.race)} · ${esc(ch.characterClass)} · Niv. ${ch.level} · CA ${ch.armorClass} · PG ${ch.hitPoints}</div>
      `;
      list.appendChild(li);
    });
  } catch (e) {
    list.innerHTML = `<li class="empty-item">${esc(e.message)}</li>`;
  }
}

// ─── Compendium ───────────────────────────────────────────────

function renderSpell(spell) {
  const card = $('spell-card');
  const desc = Array.isArray(spell.description) ? spell.description.join(' ') : (spell.description || '');
  card.innerHTML = `
    <h4>${esc(spell.name)}</h4>
    <dl>
      <dt>Nivel</dt><dd>${spell.level}</dd>
      <dt>Escuela</dt><dd>${esc(spell.school || '—')}</dd>
      <dt>Tiempo</dt><dd>${esc(spell.castingTime || '—')}</dd>
      <dt>Alcance</dt><dd>${esc(spell.range || '—')}</dd>
    </dl>
    <p class="dnd-desc">${esc(desc)}</p>
  `;
  show(card, true);
}

function renderMonster(monster) {
  const card = $('monster-card');
  card.innerHTML = `
    <h4>${esc(monster.name)}</h4>
    <dl>
      <dt>Tamaño</dt><dd>${esc(monster.size || '—')}</dd>
      <dt>Tipo</dt><dd>${esc(monster.type || '—')}</dd>
      <dt>Alineación</dt><dd>${esc(monster.alignment || '—')}</dd>
      <dt>CA</dt><dd>${monster.armorClass ?? '—'}</dd>
      <dt>PG</dt><dd>${monster.hitPoints ?? '—'}</dd>
      <dt>Desafío</dt><dd>${esc(monster.challengeRating || '—')}</dd>
    </dl>
  `;
  show(card, true);
}

// ─── Admin ────────────────────────────────────────────────────

async function loadAdmin() {
  if (!isAdmin()) return;
  loadAdminCampaigns();
  loadAdminUsers();
}

async function adminDeleteUser(userId, username) {
  const ok = await confirmModal('Desactivar usuario', `¿Desactivar la cuenta de "${username}"?`);
  if (!ok) return;
  try {
    await api(`/api/admin/users/${userId}`, { method: 'DELETE' });
    toast('Usuario desactivado');
    loadAdminUsers();
  } catch (e) {
    toast(e.message);
  }
}

async function adminSetUserRole(userId, username, newRole) {
  const label = roleLabel(newRole);
  const ok = await confirmModal('Cambiar rol', `¿Asignar rol "${label}" a "${username}"?`);
  if (!ok) return;
  try {
    await api(`/api/admin/users/${userId}/role`, {
      method: 'PATCH',
      body: JSON.stringify({ role: newRole }),
    });
    toast(`Rol actualizado a ${label}`);
    loadAdminUsers();
  } catch (e) {
    toast(e.message);
  }
}

async function adminUpdateCampaignStatus(campaignId, isPublic, isActive) {
  try {
    await api(`/api/admin/campaigns/${campaignId}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ isPublic, isActive }),
    });
    toast('Campaña actualizada');
    loadAdminCampaigns();
  } catch (e) {
    toast(e.message);
    loadAdminCampaigns();
  }
}

async function loadAdminCampaigns() {
  const wrap = $('admin-campaigns');
  const loading = $('admin-campaigns-loading');
  show(loading, true);
  wrap.innerHTML = '';
  try {
    const items = unwrap(await api('/api/admin/campaigns'));
    wrap.innerHTML = `
      <table class="data-table">
        <thead><tr><th>Nombre</th><th>Ambientación</th><th>Pública</th><th>Activa</th></tr></thead>
        <tbody>
          ${items.map((c) => `
            <tr data-campaign-id="${c.id}">
              <td>${esc(c.name)}</td>
              <td>${esc(c.setting)}</td>
              <td>
                <label class="checkbox">
                  <input type="checkbox" data-campaign-public ${c.isPublic ? 'checked' : ''} />
                  <span>${c.isPublic ? 'Sí' : 'No'}</span>
                </label>
              </td>
              <td>
                <label class="checkbox">
                  <input type="checkbox" data-campaign-active ${c.isActive ? 'checked' : ''} />
                  <span>${c.isActive ? 'Sí' : 'No'}</span>
                </label>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
    wrap.querySelectorAll('tr[data-campaign-id]').forEach((row) => {
      const id = row.dataset.campaignId;
      const publicCb = row.querySelector('[data-campaign-public]');
      const activeCb = row.querySelector('[data-campaign-active]');
      const onChange = () => adminUpdateCampaignStatus(id, publicCb.checked, activeCb.checked);
      publicCb.onchange = () => {
        publicCb.nextElementSibling.textContent = publicCb.checked ? 'Sí' : 'No';
        onChange();
      };
      activeCb.onchange = () => {
        activeCb.nextElementSibling.textContent = activeCb.checked ? 'Sí' : 'No';
        onChange();
      };
    });
  } catch (e) {
    wrap.innerHTML = `<p class="notice notice--error">${esc(e.message)}</p>`;
  } finally {
    show(loading, false);
  }
}

async function loadAdminUsers() {
  const wrap = $('admin-users');
  const loading = $('admin-users-loading');
  show(loading, true);
  wrap.innerHTML = '';
  try {
    const items = unwrap(await api('/api/admin/users'));
    const selfId = state.user?.userId;
    wrap.innerHTML = `
      <table class="data-table">
        <thead><tr><th>Usuario</th><th>Rol</th><th>Nombre</th><th>Activo</th><th>Acciones</th></tr></thead>
        <tbody>
          ${items.map((u) => {
            const isSelf = String(u.id).toLowerCase() === String(selfId || '').toLowerCase();
            const actions = isSelf
              ? '<span class="muted">—</span>'
              : u.isActive
                ? `
                  <div class="btn-group btn-group--wrap">
                    <button type="button" class="btn btn--ghost btn--sm" data-role="User" data-user-id="${u.id}" data-username="${escAttr(u.username)}">Jugador</button>
                    <button type="button" class="btn btn--ghost btn--sm" data-role="Master" data-user-id="${u.id}" data-username="${escAttr(u.username)}">DM</button>
                    <button type="button" class="btn btn--ghost btn--sm" data-role="Admin" data-user-id="${u.id}" data-username="${escAttr(u.username)}">Admin</button>
                    <button type="button" class="btn btn--danger btn--sm" data-delete-user="${u.id}" data-username="${escAttr(u.username)}">Desactivar</button>
                  </div>
                `
                : '<span class="muted">Inactivo</span>';
            const roleCls = u.role === 'Admin' ? 'badge--admin' : u.role === 'Master' ? 'badge--master' : 'badge--inactive';
            return `
            <tr>
              <td>${esc(u.username)}</td>
              <td><span class="badge ${roleCls}">${esc(roleLabel(u.role))}</span></td>
              <td>${esc(u.displayName || '—')}</td>
              <td>${u.isActive ? '✓' : '—'}</td>
              <td>${actions}</td>
            </tr>
          `;
          }).join('')}
        </tbody>
      </table>
    `;
    wrap.querySelectorAll('[data-role]').forEach((btn) => {
      btn.onclick = () => adminSetUserRole(btn.dataset.userId, btn.dataset.username, btn.dataset.role);
    });
    wrap.querySelectorAll('[data-delete-user]').forEach((btn) => {
      btn.onclick = () => adminDeleteUser(btn.dataset.deleteUser, btn.dataset.username);
    });
  } catch (e) {
    wrap.innerHTML = `<p class="notice notice--error">${esc(e.message)}</p>`;
  } finally {
    show(loading, false);
  }
}

// ─── Helpers ──────────────────────────────────────────────────

function esc(s) {
  const d = document.createElement('div');
  d.textContent = String(s ?? '');
  return d.innerHTML;
}

function escAttr(s) {
  return String(s ?? '').replace(/"/g, '&quot;');
}

function fillForm(form, data) {
  if (!form) return;
  form.querySelectorAll('input, textarea, select').forEach((el) => {
    const name = el.name;
    if (!name || !(name in data)) return;
    if (el.type === 'checkbox') el.checked = !!data[name];
    else if (data[name] !== undefined && data[name] !== null) el.value = data[name];
  });
}

function readForm(form) {
  const fd = new FormData(form);
  const out = {};
  for (const [k, v] of fd.entries()) {
    if (form.querySelector(`[name="${k}"][type="checkbox"]`)) out[k] = fd.get(k) === 'on';
    else out[k] = v;
  }
  form.querySelectorAll('input[type="checkbox"]').forEach((el) => {
    if (el.name && !(el.name in out)) out[el.name] = el.checked;
  });
  return out;
}

// ─── Events ───────────────────────────────────────────────────

document.querySelectorAll('[data-tab]').forEach((btn) => {
  btn.onclick = () => {
    document.querySelectorAll('[data-tab]').forEach((b) => b.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('[data-tab]').forEach((b) => b.setAttribute('aria-selected', String(b === btn)));
    show($('login-form'), btn.dataset.tab === 'login');
    show($('register-form'), btn.dataset.tab !== 'login');
  };
});

$('login-form').onsubmit = async (e) => {
  e.preventDefault();
  const errEl = $('auth-error');
  setText(errEl, '');
  show(errEl, false);
  try {
    const fd = new FormData(e.target);
    await login(fd.get('username'), fd.get('password'), readSessionRole(e.target));
    toast('¡Bienvenido!');
  } catch (err) {
    setText(errEl, err.message);
    show(errEl, true);
  }
};

$('register-form').onsubmit = async (e) => {
  e.preventDefault();
  const errEl = $('auth-error');
  setText(errEl, '');
  show(errEl, false);
  try {
    const fd = new FormData(e.target);
    await register(fd.get('username'), fd.get('password'), fd.get('displayName'), readSessionRole(e.target));
    toast('Cuenta creada');
  } catch (err) {
    setText(errEl, err.message);
    show(errEl, true);
  }
};

$('btn-logout').onclick = logout;

document.querySelectorAll('.nav-btn').forEach((btn) => {
  btn.onclick = () => switchView(btn.dataset.view);
});

$('campaign-form').onsubmit = async (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  try {
    await api('/api/campaigns', {
      method: 'POST',
      body: JSON.stringify({
        name: data.name,
        setting: data.setting,
        description: data.description || '',
        isPublic: !!data.isPublic,
        isActive: data.isActive !== false,
      }),
    });
    e.target.reset();
    const activeCb = e.target.querySelector('[name="isActive"]');
    if (activeCb) activeCb.checked = true;
    toast('Campaña creada');
    loadCampaigns();
  } catch (err) {
    toast(err.message);
  }
};

$('campaign-filters').onsubmit = (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  state.campaignFilters = {
    search: data.search || undefined,
    setting: data.setting || undefined,
    sortBy: data.sortBy,
    sortDir: data.sortDir,
    isPublic: data.isPublic || undefined,
    isActive: data.isActive || undefined,
  };
  loadCampaigns();
};

$('campaign-edit-form').onsubmit = async (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  try {
    const updated = await api(`/api/campaigns/${state.campaignId}`, {
      method: 'PUT',
      body: JSON.stringify({
        name: data.name,
        setting: data.setting,
        description: data.description || '',
        isPublic: !!data.isPublic,
        isActive: !!data.isActive,
      }),
    });
    state.campaign = updated;
    renderCampaignDetail(updated);
    show($('campaign-edit-form'), false);
    toast('Campaña actualizada');
    loadCampaigns();
  } catch (err) {
    toast(err.message);
  }
};

$('btn-edit-campaign').onclick = () => {
  show($('campaign-edit-form'), true);
};

$('btn-cancel-campaign-edit').onclick = () => {
  show($('campaign-edit-form'), false);
  if (state.campaign) fillForm($('campaign-edit-form'), state.campaign);
};

$('btn-delete-campaign').onclick = async () => {
  if (!state.campaign) return;
  const ok = await confirmModal('Eliminar campaña', `¿Eliminar "${state.campaign.name}" y todos sus datos?`);
  if (!ok) return;
  try {
    await api(`/api/campaigns/${state.campaignId}`, { method: 'DELETE' });
    state.campaignId = null;
    state.campaign = null;
    show($('campaign-detail'), false);
    show($('character-detail'), false);
    show($('empty-state'), true);
    toast('Campaña eliminada');
    loadCampaigns();
  } catch (err) {
    toast(err.message);
  }
};

$('character-form').onsubmit = async (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  try {
    await api(`/api/campaigns/${state.campaignId}/characters`, {
      method: 'POST',
      body: JSON.stringify(characterPayload(data, e.target)),
    });
    e.target.reset();
    const details = e.target.querySelector('details');
    if (details) details.open = false;
    toast('Personaje creado');
    loadCharacters();
    loadCampaigns();
    if (isPlayer()) loadExploreCampaigns();
  } catch (err) {
    toast(err.message);
  }
};

$('character-filters').onsubmit = (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  state.characterFilters = {
    name: data.name || undefined,
    race: data.race || undefined,
    characterClass: data.characterClass || undefined,
    isNpc: data.isNpc || undefined,
  };
  loadCharacters();
};

$('character-edit-form').onsubmit = async (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  try {
    const updated = await api(`/api/campaigns/${state.campaignId}/characters/${state.characterId}`, {
      method: 'PUT',
      body: JSON.stringify(characterPayload(data, e.target)),
    });
    state.character = normalizeCharacter(updated);
    setText($('character-title'), updated.name);
    setText($('character-meta'), `${updated.race} · ${updated.characterClass} · Nivel ${updated.level}`);
    renderSheet(updated);
    show($('character-edit-form'), false);
    toast('Ficha guardada');
    loadCharacters();
  } catch (err) {
    toast(err.message);
  }
};

$('btn-edit-character').onclick = () => {
  if (state.character) fillCharacterForm($('character-edit-form'), state.character);
  show($('character-edit-form'), true);
  show($('sheet'), false);
};

$('btn-cancel-char-edit').onclick = () => {
  show($('character-edit-form'), false);
  show($('sheet'), true);
  if (state.character) fillCharacterForm($('character-edit-form'), state.character);
};

$('btn-delete-character').onclick = async () => {
  if (!state.character) return;
  const ok = await confirmModal('Eliminar personaje', `¿Eliminar a "${state.character.name}"?`);
  if (!ok) return;
  try {
    await api(`/api/campaigns/${state.campaignId}/characters/${state.characterId}`, { method: 'DELETE' });
    state.characterId = null;
    state.character = null;
    show($('character-detail'), false);
    show($('campaign-detail'), true);
    toast('Personaje eliminado');
    loadCharacters();
  } catch (err) {
    toast(err.message);
  }
};

$('btn-back-characters').onclick = () => {
  show($('character-detail'), false);
  show($('campaign-detail'), true);
};

document.querySelectorAll('[data-char-tab]').forEach((btn) => {
  btn.onclick = () => switchCharTab(btn.dataset.charTab);
});

document.querySelectorAll('.dice-chip').forEach((btn) => {
  btn.onclick = () => {
    $('dice-expression').value = btn.dataset.expr;
  };
});

$('btn-roll').onclick = doRoll;

$('btn-upload').onclick = async () => {
  const file = $('file-input').files[0];
  if (!file) return toast('Selecciona un archivo');
  const fd = new FormData();
  fd.append('file', file);
  try {
    await api(`/api/characters/${state.characterId}/attachments`, { method: 'POST', body: fd, headers: {} });
    toast('Archivo subido');
    $('file-input').value = '';
    loadAttachments();
  } catch (err) {
    toast(err.message);
  }
};

$('spell-form').onsubmit = async (e) => {
  e.preventDefault();
  const name = new FormData(e.target).get('name');
  try {
    const spell = await api(`/api/public/dnd/spells?name=${encodeURIComponent(name)}`);
    renderSpell(spell);
  } catch (err) {
    toast(err.message);
    show($('spell-card'), false);
  }
};

$('monster-form').onsubmit = async (e) => {
  e.preventDefault();
  const name = new FormData(e.target).get('name');
  try {
    const monster = await api(`/api/public/dnd/monsters?name=${encodeURIComponent(name)}`);
    renderMonster(monster);
  } catch (err) {
    toast(err.message);
    show($('monster-card'), false);
  }
};

$('public-filters').onsubmit = async (e) => {
  e.preventDefault();
  const data = readForm(e.target);
  const grid = $('public-campaigns');
  show($('public-characters-wrap'), false);
  show(grid, true);
  grid.innerHTML = '';
  const query = buildQuery({ search: data.search, setting: data.setting, sortBy: 'name', sortDir: 'asc' });
  try {
    const campaigns = unwrap(await api(`/api/public/campaigns${query}`));
    if (!campaigns.length) {
      grid.innerHTML = '<p class="muted">Sin resultados.</p>';
      return;
    }
    campaigns.forEach((c) => {
      const card = document.createElement('div');
      card.className = 'public-card';
      card.innerHTML = `<h4>${esc(c.name)}</h4><p>${esc(c.setting)}</p>`;
      card.onclick = () => openPublicCampaign(c);
      grid.appendChild(card);
    });
  } catch (err) {
    toast(err.message);
  }
};

$('btn-back-public').onclick = () => {
  show($('public-characters-wrap'), false);
  loadPublicCampaigns();
};

$('modal-cancel').onclick = () => closeModal(false);
$('modal-confirm').onclick = () => closeModal(true);
$('modal').querySelector('.modal__backdrop')?.addEventListener('click', () => closeModal(false));

function initCharacterForms() {
  document.querySelectorAll('select[name="alignment"]').forEach((sel) => {
    DND_ALIGNMENTS.forEach((a) => {
      const opt = document.createElement('option');
      opt.value = a;
      opt.textContent = a;
      sel.appendChild(opt);
    });
  });
  mountSkillProficiencies($('skill-profs-create'), []);
  mountSkillProficiencies($('skill-profs-edit'), []);
  bindLevelProficiencySync($('character-form'));
  bindLevelProficiencySync($('character-edit-form'));
}

initCharacterForms();

// Init
if (state.token) refreshSession();
else showApp(false);
