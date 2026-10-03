# Etapa 04 — Plano de implementação

> For agentic workers: use superpowers:executing-plans to implement task-by-task.

**Goal:** cobrar serviços de galpão com saldo persistido, sem alterações parciais.
**Architecture:** cálculo separado em GarageEconomy; SaveGameStore grava estado,
saldo e compra juntos. PlaceholderSlice aplica a condição somente após sucesso.
**Tech Stack:** Unity 6, C#, uGUI, JsonUtility; sem pacotes adicionais.
**Spec:** Etapa-04-proposta-economia.md, com valores aprovados na conversa.

## Restrições e revisão

Preservar saves da versão 1; saldo em centavos; custos de R$6/L e R$10/ponto;
saldo inicial R$5000. Sem MQTT, renda ou múltiplos dias. Revisar saldo insuficiente,
gravação bloqueada, compra duplicada, crédito repetido na migração e arredondamento.

## Arquivos

- Criar Scripts/Data/GarageEconomy.cs para cálculos de custo.
- Modificar Scripts/Data/SaveGameStore.cs para transações e migração.
- Modificar Scripts/UI/PlaceholderSlice.cs para preços/saldo e ações atômicas.
- Criar Editor/GarageEconomyValidation.cs para todos os casos de revisão acima.

## Execução

- [x] Teste falha antes da implementação; compras e migração passam depois.
- [x] Interface aplica estado apenas após sucesso e mostra preços/saldo.
- [ ] Verificação no Unity e aprovação funcional antes da próxima publicação.

Valores aprovados pelo usuário: saldo de R$ 5.000,00, combustível R$ 6,00/L,
reparo R$ 10,00/ponto de desgaste. Execução nesta conversa, por etapas.

1. Adicionar validação isolada de custos, transações, migração e falha de save.
2. Separar cálculos de custo em regras de economia que trabalham em centavos.
3. Evoluir save para versão 2, migrando versão 1 sem perder estado ou data.
   Salvar compra, saldo e estado juntos; guardar o ID da compra confirmada.
4. Integrar saldo e custos ao galpão. Aplicar o serviço somente após salvar.
   Mensagens de saldo insuficiente/falha sem alteração parcial.
5. Testar no Editor e em Play, conferir persistência e documentar aprovação.

A etapa 03 foi publicada no commit 23f9037. A implementação da etapa 04 ficará
para validação funcional; será publicada ao avançar à próxima etapa.
