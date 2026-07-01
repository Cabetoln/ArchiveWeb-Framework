// O domínio Games trabalha com preços em dólar (a CheapShark devolve USD).
export function fmt(price: number, currency = 'USD') {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(price)
}
