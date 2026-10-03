# AgroSim — histórico da fundação VS01

Este documento registra a versão inicial e seus limites na época. Para o estado atual, consulte README.md na raiz, Guia-das-pastas.md e Etapa-09-acabamento-mvp.md. O MVP atual já inclui cinco operações, menus, mapa, quatro vagas, depósito com manobra, oficina, abastecimento, economia com serviços gratuitos, save persistente e ambiente Cozy Farm. A avaliação funcional do acabamento da etapa 09 fica para o final da rodada.

Abra Assets/AgroSim/Scenes/Gameplay.unity e pressione Play.
Selecione TR-01, acople a grade e inicie o preparo do solo.
O deslocamento leva 4 segundos. A operação tem 30 segundos ativos e pausa
em 45% para a decisão sobre solo compactado. Ao terminar, consulte o
resultado e volte ao galpão para repetir o ciclo.

## Implementado e validado

- Fazenda, galpão, estrada, talhão, trator e implemento com formas simples.
- Seleção obrigatória antes de iniciar; implemento visível após acoplar.
- Deslocamento, operação, evento, consequência, resultado e retorno.
- Combustível, desgaste e produtividade ilustrativos.
- Registro JSONL por operação em Application.persistentDataPath/AgroSim.
- Registros started, telemetry (aproximadamente 1 Hz, inclusive durante a
  decisão), decision e completed. Falha de escrita aparece no resultado.
- Teste do ciclo no Editor: menu AgroSim > Validar ciclo VS01.
- Teste manual em Play completou seleção, decisão de ajustar, resultado,
  retorno e reinício. Arquivo local conferido.

## Limites atuais

- Valores de demonstração; sem calibração agronômica.
- A etapa 02 preserva combustível e desgaste entre operações na mesma
  sessão e oferece abastecimento e reparo na tela exclusiva do galpão.
- A etapa 03 adiciona save separado do histórico: restaura o último estado
  confirmado no galpão e identifica tentativas interrompidas. Aprovada pelo usuário.
  O galpão exibe data e hora do último checkpoint no horário local.
- UI é criada em execução e usa a fonte padrão do Unity.
- Economia inicial implementada na etapa 04, aguardando aprovação funcional:
  saldo salvo e serviços cobrados, sem receita de colheita.
- Sem MQTT, múltiplos dias ou safra completa.

As etapas 02 e 03 foram aprovadas pelo usuário. A etapa 04 adiciona
saldo e custos de abastecimento/manutenção; veja Etapa-04-economia.md.
A progressão dos dias e os parâmetros configuráveis continuam pendentes.
