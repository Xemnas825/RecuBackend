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

function showApp(loggedIn) {
  show($('auth-section'), !loggedIn);
  show($('app-section'), loggedIn);
  show($('btn-logout'), loggedIn);
  show($('main-nav'), loggedIn);
  if (loggedIn && state.user) {
    const role = state.user.role;
    setHtml($('user-info'), `<strong>${state.user.username}</strong> · ${role}`);
    const isAdmin = role === 'Admin';
    show($('nav-admin'), isAdmin);
    if (isAdmin) $('nav-admin').classList.remove('hidden');
    else $('nav-admin')?.classList.add('hidden');
    switchView(state.view || 'campaigns');
    if (state.view === 'campaigns' || !state.view) loadCampaigns();
  }
}

async function login(username, password) {
  const data = await api('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });
  saveSession(data);
}

async function register(username, password, displayName) {
  const data = await api('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, password, displayName }),
  });
  saveSession(data);
}

function saveSession(data) {
  state.token = data.accessToken;
  state.user = { username: data.username, role: data.role, userId: data.userId };
  localStorage.setItem('token', state.token);
  localStorage.setItem('user', JSON.stringify(state.user));
  showApp(true);
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

  if (view === 'campaigns') loadCampaigns();
  if (view === 'public') loadPublicCampaigns();
  if (view === 'admin' && state.user?.role === 'Admin') loadAdmin();
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
      list.innerHTML = '<li class="empty-item">No hay campañas. Crea la primera arriba.</li>';
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
  show($('character-filters'), true);
  show($('campaign-edit-form'), false);

  try {
    const c = await api(`/api/campaigns/${id}`);
    state.campaign = c;
    renderCampaignDetail(c);
    loadCharacters();
    loadCampaigns();
  } catch (e) {
    toast(e.message);
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
    const ch = await api(`/api/campaigns/${state.campaignId}/characters/${id}`);
    state.character = ch;
    setText($('character-title'), ch.name);
    setText($('character-meta'), `${ch.race} · ${ch.characterClass} · Nivel ${ch.level}${ch.isNpc ? ' · NPC' : ''}`);
    renderSheet(ch);
    fillForm($('character-edit-form'), ch);
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

function abilityMod(score) {
  const m = Math.floor((Number(score) - 10) / 2);
  return m >= 0 ? `+${m}` : `${m}`;
}

function statBlock(name, score) {
  const v = score ?? '—';
  const mod = (score != null) ? abilityMod(score) : '—';
  return `
    <div class="stat">
      <div class="stat__abbr">${name}</div>
      <div class="stat__score">${v}</div>
      <div class="stat__mod">${mod}</div>
    </div>`;
}

function renderSheet(ch) {
  const sheet = $('sheet');
  if (!sheet) return;
  sheet.innerHTML = `
    <div class="sheet__panel">
      <div class="sheet__header">
        <h3 class="sheet__name">${esc(ch.name)}</h3>
        ${ch.isNpc ? '<span class="badge badge--npc">NPC</span>' : ''}
      </div>
      <div style="color:var(--text-2);font-size:.9rem;margin-bottom:1rem">${esc(ch.race)} &nbsp;·&nbsp; ${esc(ch.characterClass)}</div>
      <div class="kpi">
        <div class="kpi__item">
          <div class="kpi__label">Nivel</div>
          <div class="kpi__value">${ch.level}</div>
        </div>
        <div class="kpi__item">
          <div class="kpi__label">CA</div>
          <div class="kpi__value">${ch.armorClass}</div>
        </div>
        <div class="kpi__item">
          <div class="kpi__label">PG máx.</div>
          <div class="kpi__value">${ch.hitPoints}</div>
        </div>
        <div class="kpi__item">
          <div class="kpi__label">Comp.</div>
          <div class="kpi__value">+${ch.proficiencyBonus}</div>
        </div>
      </div>
    </div>
    <div class="sheet__panel">
      <div style="font-size:.72rem;font-weight:700;text-transform:uppercase;letter-spacing:.10em;color:var(--text-3);margin-bottom:.85rem">Atributos</div>
      <div class="stats">
        ${statBlock('FUE', ch.strength)}
        ${statBlock('DES', ch.dexterity)}
      </div>
    </div>
  `;
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
        try {
          const blob = await apiBlob(`/api/attachments/${a.id}`);
          const url = URL.createObjectURL(blob);
          thumb = `<img class="attachment-thumb" src="${url}" alt="" />`;
        } catch {
          thumb = '<div class="attachment-thumb">🖼</div>';
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
        downloadFile(a.id, a.fileName);
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

async function downloadFile(id, fileName) {
  try {
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
      card.innerHTML = `
        <h4>${esc(c.name)}</h4>
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
  if (state.user?.role !== 'Admin') return;
  loadAdminCampaigns();
  loadAdminUsers();
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
            <tr>
              <td>${esc(c.name)}</td>
              <td>${esc(c.setting)}</td>
              <td>${c.isPublic ? '✓' : '—'}</td>
              <td>${c.isActive ? '✓' : '—'}</td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
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
    wrap.innerHTML = `
      <table class="data-table">
        <thead><tr><th>Usuario</th><th>Rol</th><th>Nombre</th><th>Activo</th></tr></thead>
        <tbody>
          ${items.map((u) => `
            <tr>
              <td>${esc(u.username)}</td>
              <td>${esc(u.role)}</td>
              <td>${esc(u.displayName || '—')}</td>
              <td>${u.isActive ? '✓' : '—'}</td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
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
    await login(fd.get('username'), fd.get('password'));
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
    await register(fd.get('username'), fd.get('password'), fd.get('displayName'));
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
      body: JSON.stringify({
        name: data.name,
        race: data.race,
        characterClass: data.characterClass,
        level: Number(data.level),
        armorClass: Number(data.armorClass),
        hitPoints: Number(data.hitPoints),
        proficiencyBonus: Number(data.proficiencyBonus),
        strength: Number(data.strength),
        dexterity: Number(data.dexterity),
        isNpc: !!data.isNpc,
      }),
    });
    e.target.reset();
    const details = e.target.querySelector('details');
    if (details) details.open = false;
    toast('Personaje creado');
    loadCharacters();
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
      body: JSON.stringify({
        name: data.name,
        race: data.race,
        characterClass: data.characterClass,
        level: Number(data.level),
        armorClass: Number(data.armorClass),
        hitPoints: Number(data.hitPoints),
        proficiencyBonus: Number(data.proficiencyBonus),
        strength: Number(data.strength),
        dexterity: Number(data.dexterity),
        isNpc: !!data.isNpc,
      }),
    });
    state.character = updated;
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
  show($('character-edit-form'), true);
  show($('sheet'), false);
};

$('btn-cancel-char-edit').onclick = () => {
  show($('character-edit-form'), false);
  show($('sheet'), true);
  if (state.character) fillForm($('character-edit-form'), state.character);
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

// Init
if (state.token) showApp(true);
else showApp(false);
