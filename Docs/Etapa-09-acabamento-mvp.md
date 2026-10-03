# Etapa 09 — Acabamento visual do MVP

Escopo aprovado em 03/10/2026 para execução no PC de Casa, com avaliação funcional do usuário ao final.

## Entregas

- Mapa e ilustração inicial com grama, árvores e casas Cozy Farm, estradas e placas das quatro áreas.
- Galpão com piso, demarcação de quatro vagas, depósito, espaço de manobra e porta. Posições de manobra e regras de acoplamento preservadas.
- Cores, bordas, botões, seleção e ações indisponíveis consistentes entre menus, serviços, galpão e operação.
- Estrada com textura de terra, construção de origem e entrada do campo, sem bloquear o trajeto; cinco estados agrícolas preservados.
- Verificação visual e funcional em dados de teste separados do save do usuário; documentação e registro no GitHub.

## Organização

FarmTheme.cs reúne cores e aparência dos botões. FarmArtwork.cs exibe os sprites e libera as cópias temporárias usadas na repetição das texturas. FarmMenus.cs, GarageLayout.cs e FarmTravelView.cs continuam montando seus ambientes durante Play. As referências originais ficam em CozyFarmEnvironment.asset; arquivos do fornecedor não são alterados.

## Testes isolados

No Unity, AgroSim > Testes > Play com save isolado inicia uma sessão com diretório temporário próprio. O aviso TESTE ISOLADO aparece no topo. Save, compras e telemetria vão para esse diretório. Ao sair de Play a opção é removida; Play normal volta a usar o save original. O executável não contém essa opção de Editor.

Retomar último save de teste reabre a pasta temporária da última sessão neste Editor, permitindo conferir persistência e tentativas interrompidas.

## Verificação

Conferências realizadas no Unity 6000.6.4f1, em 03/10/2026:

- Dez verificações automáticas aprovadas: campo, galpão, menus, manobras, manutenção, save, telemetria de retorno, enquadramento, ciclo VS01 e economia.
- Ciclo manual completo das cinco operações, com seleção, acoplamento, saída pela estrada, decisão, resultado e retorno ao galpão. No save isolado final: cinco operações confirmadas, próxima etapa 5 (ciclo concluído), 77,5 L, desgaste 18%, produtividade 100% e saldo R$ 5.000.
- Telemetria da sessão: cinco conclusões, cinco confirmações, cinco decisões, 302 amostras, um abastecimento e uma tentativa interrompida. Nenhum identificador de evento duplicado.
- Interrupção durante preparo do solo aos 45%: a retomada restaurou o checkpoint de 100 L, desgaste zero e solo intacto; registrou a tentativa como interrompida uma vez. A operação refeita recebeu um identificador diferente.
- Abastecimento e reparo em sessões isoladas: valores atualizados, custo zero, botões indisponíveis quando tanque cheio ou desgaste zero.
- Aparência conferida em Free Aspect, 16:9 e 16:10. Galpão, mapa, serviços, decisão, resultado e visão completa do talhão permaneceram legíveis. O enquadramento da estrada foi ampliado para incluir as construções das extremidades.

Os testes utilizaram pastas temporárias. A comparação dos arquivos originais de save e histórico confirma que o progresso do usuário foi preservado. Nenhum pacote novo foi instalado e os arquivos originais do Cozy Farm permaneceram intactos.

A avaliação visual e funcional do usuário encerra esta etapa antes de escolher a próxima. Trator, implementos, bomba e ferramentas ainda são placeholders; os parâmetros e a evolução agrícola continuam ilustrativos. Não foi gerado um novo executável nesta etapa.
