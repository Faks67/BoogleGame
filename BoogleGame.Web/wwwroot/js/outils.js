// Appels API, échappement HTML, notifications, stockage local

export async function api(url, corps) {
  const reponse = await fetch(url, corps === undefined ? {} : {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(corps)
  });
  const donnees = await reponse.json().catch(() => null);
  if (!reponse.ok) throw new Error(donnees?.erreur || 'Le serveur ne répond pas.');
  return donnees;
}

export function esc(valeur) {
  return String(valeur ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

export function toast(message) {
  const el = document.createElement('div');
  el.className = 'toast';
  el.textContent = message;
  const zone = document.getElementById('toasts');
  zone.replaceChildren(el);
  setTimeout(() => el.remove(), 2200);
}

// ---------- Stockage local (réglages, records, défi du jour) ----------
function lire(cle, defaut) {
  try {
    const v = localStorage.getItem(cle);
    return v ? JSON.parse(v) : defaut;
  } catch {
    return defaut;
  }
}
function ecrire(cle, valeur) {
  try { localStorage.setItem(cle, JSON.stringify(valeur)); } catch { /* stockage indisponible */ }
}

export const reglages = {
  lire: () => ({ langue: 'FR', taille: 4, duree: 180, joueurs: ['Joueur 1', 'Joueur 2'], ...lire('boggle.reglages', {}) }),
  ecrire: r => ecrire('boggle.reglages', r)
};

const cleRecord = r => `${r.langue}-${r.taille}-${r.duree}`;
export const records = {
  lire: r => lire('boggle.records', {})[cleRecord(r)] || [],
  // Ajoute un score et renvoie sa place (1 = meilleur) ou 0 s'il n'entre pas dans le top 5
  ajouter(r, entree) {
    const tous = lire('boggle.records', {});
    const liste = [...(tous[cleRecord(r)] || []), entree].sort((a, b) => b.score - a.score || b.mots - a.mots);
    const place = liste.indexOf(entree) + 1;
    tous[cleRecord(r)] = liste.slice(0, 5);
    ecrire('boggle.records', tous);
    return place <= 5 ? place : 0;
  }
};

export const defis = {
  lire: (date, langue) => lire('boggle.defis', {})[`${date}-${langue}`] || null,
  ecrire(date, langue, resultat) {
    const tous = lire('boggle.defis', {});
    const cle = `${date}-${langue}`;
    if (!tous[cle] || resultat.score > tous[cle].score) tous[cle] = resultat;
    ecrire('boggle.defis', tous);
  }
};
