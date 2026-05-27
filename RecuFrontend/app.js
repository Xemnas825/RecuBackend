const API_BASE = '__API_BASE__';

const state = {
  token: localStorage.getItem('token') || '',
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  campaignId: null,
  characterId: null,
};

const $ = (id) => document.getElementById(id);

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
  return res.json();
}

function showApp(loggedIn) {
  $('auth-section').classList.toggle('hidden', loggedIn);
  $('app-section').classList.toggle('hidden', !loggedIn);
  $('btn-logout').classList.toggle('hidden', !loggedIn);
  if (loggedIn && state.user) {
    $('user-info').textContent = `${state.user.username} (${state.user.role})`;
    loadCampaigns();
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
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  showApp(false);
}

async function loadCampaigns() {
  const campaigns = await api('/api/campaigns?sortBy=name&sortDir=asc');
  const list = $('campaign-list');
  list.innerHTML = '';
  (campaigns.value ?? campaigns).forEach((c) => {
    const li = document.createElement('li');
    li.textContent = `${c.name} (${c.setting})${c.isPublic ? ' 🌐' : ''}`;
    li.onclick = () => openCampaign(c);
    list.appendChild(li);
  });
}

function openCampaign(c) {
  state.campaignId = c.id;
  $('campaign-title').textContent = c.name;
  $('campaign-detail').classList.remove('hidden');
  loadCharacters();
}

async function loadCharacters() {
  const chars = await api(`/api/campaigns/${state.campaignId}/characters?sortBy=name`);
  const list = $('character-list');
  list.innerHTML = '';
  (chars.value ?? chars).forEach((ch) => {
    const li = document.createElement('li');
    li.textContent = `${ch.name} – ${ch.race} ${ch.characterClass} (niv.${ch.level})${ch.isNpc ? ' [NPC]' : ''}`;
    li.onclick = () => openCharacter(ch);
    list.appendChild(li);
  });
}

function openCharacter(ch) {
  state.characterId = ch.id;
  $('character-title').textContent = ch.name;
  $('character-detail').classList.remove('hidden');
  $('roll-result').textContent = '';
  $('spell-result').textContent = '';
  loadAttachments();
}

async function loadAttachments() {
  const items = await api(`/api/characters/${state.characterId}/attachments`);
  const list = $('attachment-list');
  list.innerHTML = '';
  (items.value ?? items).forEach((a) => {
    const li = document.createElement('li');
    li.textContent = `${a.fileName} (${a.sizeBytes} bytes)`;
    list.appendChild(li);
  });
}

// Events
document.querySelectorAll('[data-tab]').forEach((btn) => {
  btn.onclick = () => {
    document.querySelectorAll('[data-tab]').forEach((b) => b.classList.remove('active'));
    btn.classList.add('active');
    $('login-form').classList.toggle('hidden', btn.dataset.tab !== 'login');
    $('register-form').classList.toggle('hidden', btn.dataset.tab !== 'register');
  };
});

$('login-form').onsubmit = async (e) => {
  e.preventDefault();
  $('auth-error').textContent = '';
  try {
    const fd = new FormData(e.target);
    await login(fd.get('username'), fd.get('password'));
  } catch (err) {
    $('auth-error').textContent = err.message;
  }
};

$('register-form').onsubmit = async (e) => {
  e.preventDefault();
  $('auth-error').textContent = '';
  try {
    const fd = new FormData(e.target);
    await register(fd.get('username'), fd.get('password'), fd.get('displayName'));
  } catch (err) {
    $('auth-error').textContent = err.message;
  }
};

$('btn-logout').onclick = logout;

$('campaign-form').onsubmit = async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  await api('/api/campaigns', {
    method: 'POST',
    body: JSON.stringify({
      name: fd.get('name'),
      setting: fd.get('setting'),
      description: '',
      isPublic: fd.get('isPublic') === 'on',
      isActive: true,
    }),
  });
  e.target.reset();
  loadCampaigns();
};

$('character-form').onsubmit = async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  await api(`/api/campaigns/${state.campaignId}/characters`, {
    method: 'POST',
    body: JSON.stringify({
      name: fd.get('name'),
      race: fd.get('race'),
      characterClass: fd.get('characterClass'),
      level: Number(fd.get('level')),
      armorClass: 10,
      hitPoints: 10,
      proficiencyBonus: 2,
      strength: 10,
      dexterity: 10,
      isNpc: fd.get('isNpc') === 'on',
    }),
  });
  e.target.reset();
  loadCharacters();
};

$('btn-roll').onclick = async () => {
  const expr = $('dice-expression').value;
  const mode = Number($('d20-mode').value);
  const roll = await api(`/api/characters/${state.characterId}/rolls`, {
    method: 'POST',
    body: JSON.stringify({ label: 'Tirada web', diceExpression: expr, d20Mode: mode }),
  });
  $('roll-result').textContent = `🎲 Total: ${roll.total} (${roll.rawResults})${roll.isCritical ? ' ¡CRÍTICO!' : ''}`;
};

$('btn-spell').onclick = async () => {
  const name = $('spell-name').value;
  const spell = await api(`/api/public/dnd/spells?name=${encodeURIComponent(name)}`);
  $('spell-result').textContent = JSON.stringify(spell, null, 2);
};

$('btn-upload').onclick = async () => {
  const file = $('file-input').files[0];
  if (!file) return alert('Selecciona un archivo');
  const fd = new FormData();
  fd.append('file', file);
  await api(`/api/characters/${state.characterId}/attachments`, { method: 'POST', body: fd, headers: {} });
  loadAttachments();
};

$('btn-back-campaigns').onclick = () => {
  $('campaign-detail').classList.add('hidden');
  $('character-detail').classList.add('hidden');
};

$('btn-back-characters').onclick = () => $('character-detail').classList.add('hidden');

if (state.token) showApp(true);
else showApp(false);
