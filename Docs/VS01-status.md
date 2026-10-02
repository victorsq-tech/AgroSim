# AgroSim — VS01 com placeholders

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
- O registro local é histórico; não restaura o estado do jogo ao abrir.
- UI é criada em execução e usa a fonte padrão do Unity.
- Sem MQTT, economia, múltiplos dias ou safra completa.

A etapa 02 foi aprovada pelo usuário. A progressão dos dias
e os parâmetros configuráveis continuam pendentes.
