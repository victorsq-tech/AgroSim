# Proposta da etapa 03 — salvar e retomar o trator

Status: proposta para revisão do usuário; implementação não iniciada.

## Objetivo

Manter combustível e desgaste do TR-01 ao fechar e reabrir o jogo.
Isso permite continuar o ciclo de operação, abastecimento e manutenção
sem perder a condição do equipamento ao encerrar uma sessão.

## Comportamento proposto

- Salvar automaticamente ao voltar ao galpão e após abastecer ou reparar.
- Criar o primeiro ponto salvo ao iniciar uma partida nova no galpão.
- Ao abrir, restaurar o último ponto salvo e mostrar a tela do galpão.
- Restaurar combustível e desgaste. A seleção do trator e o acoplamento
  devem ser feitos novamente, preservando a preparação já aprovada.
- Mostrar confirmação discreta de salvamento ou mensagem de falha.
- Oferecer “Nova partida”, com confirmação antes de substituir o save.
- Se não houver save, iniciar com 100 L e 0% de desgaste.
- Se o save estiver inválido, mostrar uma mensagem, preservar uma cópia
  do arquivo e permitir iniciar um estado seguro.

O salvamento será local no PC, separado da telemetria JSONL e fora do Git.
Esta etapa não terá nuvem, MQTT, múltiplos perfis, economia ou safra completa.

## Limite importante

O ponto salvo será o galpão. Se o jogo for fechado durante uma operação,
ao abrir novamente ele voltará ao último estado salvo no galpão; não haverá
retomada no meio do talhão. Essa escolha precisa ser confirmada pelo usuário.

## Sequência de implementação, após aprovação

1. Criar um formato de save com versão, identificação do trator,
   combustível e desgaste; validar números finitos e limites de 0–100.
2. Implementar leitura e escrita local com arquivo temporário e substituição
   após sucesso, mantendo o save anterior se a gravação falhar.
3. Integrar a restauração ao abrir o galpão e o salvamento aos três pontos:
   primeiro início, retorno ao galpão e manutenção.
4. Adicionar mensagens de status e confirmação de nova partida.
5. Testar no Editor e pedir validação funcional do usuário.
6. Registrar no GitHub somente após essa validação.

## Critérios de aceitação

- Concluir uma operação, voltar ao galpão e reabrir: combustível e desgaste
  devem coincidir com os valores anteriores.
- Abastecer e reabrir: tanque cheio e desgaste anterior preservado.
- Reparar e reabrir: desgaste zerado e combustível anterior preservado.
- Sem save: galpão com 100 L e 0% de desgaste.
- Save truncado, versão desconhecida ou valores fora dos limites: mensagem
  compreensível, arquivo preservado e nenhuma exceção bloqueando o jogo.
- Falha de gravação: aviso visível e save anterior preservado.
- Cancelar nova partida: nenhum dado alterado.
- Confirmar nova partida: novo estado de 100 L e 0%, salvo localmente.

## Etapas posteriores sugeridas

Após validar o salvamento, planejar custos de combustível e manutenção,
e depois progressão das operações/dias conforme o desenho do MVP.
Essas funcionalidades exigem novas decisões sobre regras e balanceamento.
