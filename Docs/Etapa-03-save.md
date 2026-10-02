# Etapa 03 — Save local e identificação de tentativas

Status: aprovada pelo usuário, incluindo data/hora do último save; publicação
autorizada ao iniciar o planejamento da etapa 04.

Fluxo de trabalho: ao avançar para uma nova etapa, registrar no GitHub a etapa
anterior validada. Essa autorização permanece válida nas próximas transições.

O checkpoint conserva combustível e desgaste confirmados no galpão. É salvo na
primeira sessão, depois de abastecer/reparar e ao retornar de uma operação.
Na abertura, seleções são limpas e o trator volta ao galpão. Fechar durante uma
operação ou antes do retorno restaura o último checkpoint, sem retomar o percurso.
O checkpoint também guarda `savedAtUtc`; o galpão mostra o último save no horário
local, no formato `dd/MM/yyyy HH:mm:ss`. Metadados de tentativas e gravações que
falham não alteram essa data. Saves anteriores sem esse campo continuam válidos
e mostram data indisponível até a próxima confirmação de progresso.

O arquivo versionado `save.json` fica em `Application.persistentDataPath/AgroSim`.
Gravação usa arquivo temporário, flush e substituição com backup `.bak`.
Falhas preservam o checkpoint anterior, exibem aviso e bloqueiam nova saída até
uma tentativa de gravação bem-sucedida. Saves inválidos são copiados para
`save.json.invalid-<id>` antes de iniciar com 100 L e desgaste zero.
Se a cópia não puder ser preservada, a gravação permanece bloqueada.

Cada saída cria um `operationId` exclusivo vinculado ao `runId` e ao checkpoint
de origem. Metadados da tentativa são persistidos antes da saída e na conclusão,
sem atualizar combustível/desgaste do checkpoint. A confirmação no galpão aplica
o estado e registra o ID no mesmo arquivo; repetir essa confirmação não reaplica
o resultado. Em uma queda entre essa gravação e o registro JSONL, a lista
`confirmedIds` do save é a autoridade para reconciliar a confirmação.

Ao reabrir, uma tentativa pendente gera `interrupted` ou
`completed_unconfirmed` no JSONL original. Esse evento tem ID determinístico
para recuperação idempotente. A nova tentativa recebe outro ID.
Telemetria local continua aproximadamente a 1 Hz; MQTT permanece pendente.
Leitores futuros devem ignorar linhas vazias/incompletas e separar estados de
tentativas de amostras de telemetria. A lista de confirmações cresce com o uso;
compactação e sincronização entre dispositivos não fazem parte desta etapa.

Novo jogo exige confirmação e cria outra sessão com 100 L e desgaste zero,
mantendo históricos JSONL. Cancelar mantém o progresso.

## Verificação

Menu `AgroSim > Validar save e tentativas`: testes isolados em diretório temporário
para restauração, IDs, interrupção, conclusão sem confirmação, confirmação
idempotente, manutenção, dados inválidos, versão desconhecida e falha de gravação.
Os validadores existentes do ciclo, seleção e manutenção também devem passar.

Teste manual no Unity confirmou cancelamento de Novo jogo, interrupção em
aproximadamente 40% e recuperação do checkpoint de 100 L/0%, com evento
`interrupted` no histórico original. Uma nova tentativa completou o ciclo com
ajuste de velocidade e confirmou no galpão 77,5 L e 3,6% de desgaste.
Após parar e reiniciar Play, os mesmos valores reapareceram, com seleções limpas.
Os quatro validadores passaram após a compilação final. O aviso de falha de
gravação durante o validador é intencional: ele simula uma substituição bloqueada.

## Roteiro de aprovação funcional

1. Complete uma operação e retorne ao galpão. Pare Play e inicie novamente:
   combustível e desgaste devem reaparecer iguais, com seleções limpas.
2. Abasteça/repare e repita a abertura para conferir a persistência.
3. Inicie uma operação e pare Play antes de voltar ao galpão. Ao reabrir,
   confira o aviso de recuperação e o estado do último galpão salvo.
4. Teste Novo jogo > Cancelar e, quando quiser reiniciar, Confirmar novo jogo.

Valores continuam ilustrativos. Save local não é armazenamento na nuvem.
