# Etapa 06 — Menus e visão geral da fazenda

Escopo aprovado: menu inicial com Continuar, Novo jogo e Sair; mapa ilustrado
clicável para galpão, oficina/mecânico, abastecimento e talhão; resumo da
máquina, saldo, próxima operação e último save. Cada área volta ao mapa.

Status: implementação local; aguardar confirmação funcional antes da etapa seguinte.
Etapa 05 publicada em 5663429 ao iniciar esta etapa, conforme combinado.

## Fluxo

- O jogo abre no menu inicial. Continuar usa o checkpoint já carregado e abre
  a visão geral. Novo jogo reutiliza a confirmação existente; cancelar não
  altera o progresso, confirmar reinicia o save e abre o mapa.
- O mapa mostra estradas, galpão, oficina, posto, talhão, árvores e lago com
  formas locais. Apenas as quatro áreas principais têm ações; a miniatura
  decorativa do menu inicial não é clicável.
- Galpão mantém preparação e operação existentes e ganha Voltar ao mapa.
- Oficina repara o TR-01; abastecimento completa seu tanque. Ambos usam a
  gravação atômica e custos zero existentes. O único trator do protótipo é
  selecionado automaticamente ao realizar o serviço. Progresso e saldo são
  preservados e o resumo reflete a condição atualizada.
- Talhão mostra estado confirmado e produtividade e conduz ao galpão para
  preparar trator/implemento. Clicar no talhão não inicia trabalho diretamente.
- Durante deslocamento, operação, resultado e retorno, os menus ficam ocultos
  e a navegação fica bloqueada. Ao retornar e confirmar, abre o galpão.
- Sair possui confirmação. No Editor encerra Play; no executável fecha o jogo.
  Mantém o último save confirmado. Não fecha o Unity nem outros aplicativos.

## Estrutura

FarmNavigation guarda o fluxo das telas; FarmMenus constrói a apresentação
uGUI no Canvas existente. PlaceholderSlice expõe somente dados e ações locais
para a navegação. A cena e os prefabs existentes ficam preservados; não foram
adicionados pacotes nem assets externos. O save continua na versão 3.

## Validação e roteiro do usuário

Oito validadores passaram após compilar: seleção, manutenção, economia, save,
motor de operação, progressão, telemetria após retorno e navegação dos menus.
O teste de navegação verificou o menu inicial, ida/volta pelas quatro áreas,
bloqueio de início pelo mapa e bloqueio durante operação.

1. Abra Play: deve mostrar AGROSIM com as três opções iniciais.
2. Continuar: confira dados e clique nas quatro áreas do mapa.
3. Oficina e abastecimento: realize os serviços quando disponíveis e confira
   condição, saldo, mensagens e data atualizados ao voltar ao mapa.
4. Talhão: confira terreno e próxima operação; use Preparar no galpão.
5. Prepare trator/implemento e faça uma operação. Ao confirmar no galpão,
   volte ao mapa e confira nova etapa e condição.
6. Menu inicial > Novo jogo > Cancelar preserva o save. Confirmar reinicia.
7. Sair > Cancelar continua. Sair confirmado encerra apenas o jogo/Play.

Visuais são placeholders. Melhorias futuras: acabamento do galpão/oficina,
sprites dos prédios, ícones, animação de serviços e arte do mapa. Não há loja,
nova economia, personagem mecânico controlável ou nova simulação nesta etapa.
Conferência no Editor: menu inicial, cancelamento de Novo jogo, acessos ao mapa, oficina, abastecimento, talhão e galpão verificados. Plantio concluído e confirmado; retorno ao mapa mostrou Fertilização (3/5), 77,5 L, 3,6% de desgaste, produtividade 85%, saldo R$ 5.000 e último save atualizado. O teste avançou o checkpoint local até Fertilização. Console sem erros durante a conferência. Exportação de executável e encerramento pelo botão Sair não foram conferidos nesta rodada.
