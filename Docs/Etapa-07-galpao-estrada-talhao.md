# Etapa 07 — Galpão, estrada e talhão

Status: implementação local, aguardando confirmação funcional. A etapa 06 foi publicada no GitHub no commit c2e539e ao iniciar esta etapa.

## Desenho aprovado
Quatro vagas, TR-01 na primeira e três vagas vazias. Depósito dos cinco implementos com botões abaixo dos equipamentos. Selecionar o implemento da próxima etapa inicia a manobra: vaga → depósito → acoplamento → porta. Somente na porta é liberada a saída. Guardar faz o caminho inverso; ao voltar da operação, os equipamentos já estão armazenados.

A parte inferior reúne seleção, combustível, desgaste, saldo, produtividade, operação e mensagens. Os serviços continuam nas telas Oficina e Abastecimento acessíveis pelo mapa.

## Áreas
- Galpão: layout uGUI com silhuetas locais, vagas e manobra animada.
- Estrada: ambiente exclusivo ligando galpão e entrada do talhão, percorrido na ida e volta.
- Talhão: FarmGround reaproveitado no terreno, ampliado e enquadrado para trabalho em tela dedicada. As quatro passadas mantêm a cobertura e os estados das cinco operações. Visita pelo mapa mostra o terreno confirmado sem trator em trabalho.

Os ajustes de posição/visibilidade são feitos durante a execução. A cena e os prefabs originais não foram sobrescritos. Sem pacotes ou assets externos. Save e registros permanecem no fluxo existente; manobras não criam operações ou consumo.

## Verificação
Manobra testada primeiro com falha por ausência do componente, depois aprovada: acoplamento antes da porta, saída bloqueada, rejeição de pedidos simultâneos, armazenamento antes de estacionar e reset de recuperação. Enquadramento testado em retrato, 4:3, 16:9 e janela larga, com reserva do painel inferior e cabeçalho da visita ao talhão.

No Editor: seleção, acoplamento, espera na porta, desacoplamento/armazenamento, estrada de ida, terreno ampliado, evento, conclusão, estrada de volta e equipamentos guardados conferidos. A fertilização do save local foi concluída e confirmada: próxima operação Pulverização (4/5), combustível 55 L, desgaste 7,2%, produtividade 85%, saldo R$ 5.000.

Executável exportado não foi testado nesta rodada. Visuais e velocidade da manobra são placeholders.
Dez validadores passaram após a compilação final. A câmera desenha o quadro inteiro para limpar a imagem anterior e ajusta o enquadramento quando o tamanho da tela muda. A legenda fica dentro do painel inferior, sem cobrir o terreno. Revisão encontrou o trator estacionado aparecendo no campo; corrigido ocultando-o fora de operação, conferido pela visita ao talhão.
