import { persistentAtom } from '@nanostores/persistent';
import { computed } from 'nanostores';

export interface UserData {
  token: string;
  role: string;
  name: string;
  negocioId?: string;
  sucursalId?: string;
}

export const $userStore = persistentAtom<UserData | null>('userStore', null, {
  encode: JSON.stringify,
  decode: JSON.parse,
});

export const $authToken = computed($userStore, user => user?.token || null);

export function setLoginData(data: UserData) {
  $userStore.set(data);
}

export function logout() {
  $userStore.set(null);
}

export function isAuthenticated(): boolean {
  const user = $userStore.get();
  return user !== null && !!user.token;
}