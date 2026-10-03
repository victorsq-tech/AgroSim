# Etapa 04 — Economia do galpão

Status: aprovada pelo usuário na transição para a etapa 05.
Etapa anterior publicada: 23f9037. Publicação autorizada nesta transição.
Os custos de serviços foram ajustados para zero conforme pedido; a estrutura
de economia permanece disponível para futura ativação.

## Comportamento

- Saldo inicial R$ 5.000,00; combustível e reparo gratuitos nesta fase.
- Galpão mostra saldo, preços unitários e total de cada serviço antes de agir.
- Compra salva saldo, máquina, data e ID juntos; a interface atualiza depois
  do sucesso. Falha não dá serviço nem cobra. Sem saldo, serviço fica bloqueado
  e um aviso explica o motivo. Não há cobrança de combustível ao retornar.
- Centavos inteiros, arredondamento do total para o centavo mais próximo;
  metade de centavo arredonda para cima. IDs persistidos evitam cobrar uma
  compra novamente, inclusive depois de outros serviços.
- Save versão 2; versão 1 migra com saldo inicial uma única vez, preservando
  condição, sessão, checkpoint, tentativas e data do último progresso.
- Compras confirmadas ficam também em `purchases.jsonl`, separado da telemetria.
  O save é a autoridade se esse histórico falhar. Histórico de compras não
  é reimportado para calcular saldo. A lista de IDs cresce; compactação futura.
- Novo jogo confirmado reinicia saldo e máquina, mantendo os históricos.

Os parâmetros ficam em `Scripts/Data/GarageEconomy.cs`. Ainda são valores
ilustrativos e não existe renda, empréstimo, compra de equipamentos ou colheita.

## Verificação

Menu AgroSim > Validar economia do galpão: custos e frações, saldo exato,
insuficiente, serviços desnecessários, duplicação após outro serviço, persistência,
falha de substituição do save, migração sem crédito repetido, operações sem
novo débito, interrupção e novo jogo. Testes usam diretórios temporários isolados.
Avisos nos testes de falha de gravação são intencionais.

Teste manual no Unity: save anterior com 77,5 L/3,6% abriu com R$ 5.000.
Abastecer custou R$ 135,00 e manteve desgaste; reparar custou R$ 36,00 e manteve
combustível. Saldo resultante R$ 4.829,00, tanque de 100 L e desgaste zero.
Após parar/reiniciar Play, saldo, condição e data reapareceram iguais.
Os cinco validadores passaram após a última compilação, incluindo preservação
do saldo na confirmação e na recuperação de uma operação.

## Roteiro de aprovação

1. Confira saldo e preços no galpão. Selecione TR-01 e acople a grade.
2. Faça uma operação e retorne; saldo permanece, combustível/desgaste mudam.
3. Confira os custos; abasteça/repare e verifique saldo e condição.
4. Pare e reinicie Play; saldo, condição e data devem reaparecer iguais.
5. Novo jogo > Cancelar mantém os valores. Confirmar reinicia com R$ 5.000.

Progressão de operações/dias será planejada após esta aprovação. MQTT e
melhorias visuais continuam pendentes.

Ajuste autorizado: valores ativos zero. Validador verifica serviços gratuitos sem alterar saldo; preços anteriores usados apenas em testes isolados da estrutura financeira.
