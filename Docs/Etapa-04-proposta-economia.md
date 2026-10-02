# Proposta da etapa 04 — Saldo e custos do galpão

Status: planejamento preparado; regras e valores apresentados para revisão.
Implementação ainda não iniciada. Etapa 03 aprovada e autorizada para publicação.

## Objetivo

Adicionar uma consequência financeira às ações já disponíveis no galpão:
abastecer e reparar. O jogador verá saldo, preço e custo total antes de agir.
O saldo e a condição do trator devem permanecer consistentes ao salvar/reabrir.

## Valores propostos

- Saldo inicial: R$ 5.000,00.
- Combustível: R$ 6,00 por litro reposto.
- Reparo: R$ 10,00 por ponto percentual de desgaste removido.

São parâmetros de demonstração; não representam pesquisa de preços reais.
O usuário pode ajustar esses valores antes da implementação.

## Regras

1. Abastecer completa até 100 L e cobra somente os litros faltantes.
   Exemplo: 77,5 L para 100 L custa R$ 135,00.
2. Reparar zera o desgaste e cobra o desgaste atual.
   Exemplo: 3,6% para zero custa R$ 36,00.
3. Com saldo insuficiente, a ação é bloqueada com explicação e não altera
   combustível, desgaste, saldo ou data do último save.
4. Tanque cheio/desgaste zero continuam sem ação disponível e sem cobrança.
5. Valores monetários usam centavos inteiros. O total é arredondado uma vez
   na transação, com regra consistente para frações de litro/desgaste.
6. Saldo e estado do trator são gravados juntos; somente após uma gravação
   bem-sucedida a interface aplica a compra. Falha não conclui a compra.
7. Consumo durante a operação não gera outra cobrança: o combustível foi
   comprado no abastecimento. Retorno confirma a condição e preserva o saldo.
8. Fechar durante operação mantém a regra do checkpoint no galpão. As compras
   anteriormente confirmadas permanecem; a tentativa interrompida não duplica
   débito nem confirmação.
9. Novo jogo com confirmação reinicia saldo e máquina. Não adicionar receitas,
   empréstimos, compra de equipamentos ou custo de oficina nesta etapa.

## Integração

- Criar regras de custo separadas da interface, com parâmetros ajustáveis.
- Mostrar saldo no galpão e custo total nos serviços de abastecimento/reparo.
- Evoluir o formato de save para incluir saldo; migrar saves da etapa 03
  preservando combustível, desgaste, IDs e data, e creditando saldo inicial
  uma única vez. Após migrado, reabrir não repete o crédito.
- Registrar compras confirmadas com identificador próprio, sem confundi-las
  com as amostras de telemetria por operação. Persistência é a autoridade
  caso o histórico falhe após uma compra confirmada.
- Manter histórico existente, preparação dos equipamentos e ciclo do talhão.

## Verificação e aprovação

Testar cálculo/arre­dondamento, saldo exato e insuficiente, compra sem alteração
necessária, falha de gravação, repetição de confirmação, reabertura, migração
sem crédito duplicado e reset confirmado/cancelado. Revalidar save e ciclo atual.

Após implementação e teste no Unity, aguardar a confirmação funcional do usuário.
Na transição para a etapa seguinte, fazer commit/push automaticamente conforme
a autorização permanente do usuário.

## Limites e sequência posterior

O orçamento serve para experimentar custos; ainda não há renda de colheita.
Progressão de operações/dias será a próxima proposta após validar esta entrega.
MQTT e melhorias visuais do galpão continuam nos respectivos itens pendentes.
