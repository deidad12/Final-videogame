import sys
import subprocess
import os

# Auto-instalar reportlab si no está instalado
try:
    import reportlab
except ImportError:
    print("ReportLab no está instalado. Instalándolo automáticamente...")
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "reportlab"])
        import reportlab
    except Exception as e:
        print(f"No se pudo instalar reportlab automáticamente: {e}")
        print("Por favor instala reportlab manualmente usando: pip install reportlab")
        sys.exit(1)

from reportlab.lib.pagesizes import letter
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib import colors

def generate():
    pdf_filename = "Documentacion_Pastel_Dream_World.pdf"
    
    # Configurar el documento con márgenes de 40pt
    doc = SimpleDocTemplate(
        pdf_filename,
        pagesize=letter,
        rightMargin=40,
        leftMargin=40,
        topMargin=40,
        bottomMargin=40
    )
    
    styles = getSampleStyleSheet()
    
    # Colores pastel de la guía estética
    c_title = colors.HexColor("#6D3B47")      # Frambuesa oscuro / Vino pastel
    c_text = colors.HexColor("#2C1E21")       # Chocolate oscuro (casi negro)
    c_accent = colors.HexColor("#E297A6")     # Rosa medio / Rosa viejo
    c_bg_panel = colors.HexColor("#FFF0F3")   # Fondo de tabla rosa pálido
    
    title_style = ParagraphStyle(
        'DocTitle',
        parent=styles['Heading1'],
        fontName='Helvetica-Bold',
        fontSize=22,
        textColor=c_title,
        spaceAfter=15,
        alignment=1 # Centrado
    )
    
    h1_style = ParagraphStyle(
        'H1',
        parent=styles['Heading2'],
        fontName='Helvetica-Bold',
        fontSize=13,
        textColor=c_title,
        spaceBefore=14,
        spaceAfter=6,
        keepWithNext=True
    )

    h2_style = ParagraphStyle(
        'H2',
        parent=styles['Heading3'],
        fontName='Helvetica-Bold',
        fontSize=10,
        textColor=c_accent,
        spaceBefore=8,
        spaceAfter=4,
        keepWithNext=True
    )
    
    body_style = ParagraphStyle(
        'Body',
        parent=styles['BodyText'],
        fontName='Helvetica',
        fontSize=9.5,
        textColor=c_text,
        spaceAfter=7,
        leading=13.5
    )
    
    bullet_style = ParagraphStyle(
        'Bullet',
        parent=body_style,
        leftIndent=15,
        firstLineIndent=-10,
        spaceAfter=3
    )
    
    story = []
    
    # Encabezado espaciador inicial
    story.append(Spacer(1, 10))
    story.append(Paragraph("DOCUMENTACIÓN DE DISEÑO: PASTEL DREAM WORLD", title_style))
    story.append(Spacer(1, 5))
    
    # 1. Ficha Técnica
    story.append(Paragraph("1. Ficha Técnica", h1_style))
    story.append(Paragraph("<b>Nombre del Videojuego:</b> Pastel Dream World", body_style))
    story.append(Paragraph("<b>Tipo de Juego (Género):</b> Plataformas 2D de Precisión (Estilo Mario)", body_style))
    story.append(Paragraph("<b>Clasificación de Edad:</b> PEGI 3 / Clasificación A (Apto para todo público)", body_style))
    story.append(Paragraph("<b>Tecnología:</b> Unity / C#", body_style))
    story.append(Paragraph("<b>Estética Visual:</b> Colores rosa pastel, lila, celeste y diseño suave", body_style))
    story.append(Spacer(1, 5))
    
    # 2. Historia
    story.append(Paragraph("2. Historia del Videojuego", h1_style))
    story.append(Paragraph("En el pacífico reino de <i>Lollipop Valley</i>, todo era armonía, risas y colores pasteles. Sus habitantes vivían felices rodeados de montañas de algodón de azúcar y ríos de jarabe de fresa dulce.", body_style))
    story.append(Paragraph("Sin embargo, una misteriosa fuerza de tonos industriales oscuros empezó a robar la dulzura del reino. El malvado emperador <b>Dr. Cocoa</b> y sus secuaces se apoderaron de las fuentes de caramelo del reino.", body_style))
    story.append(Paragraph("Nuestra heroína, armada con su agilidad para saltar sobre las cabezas de los invasores y su gran valentía, debe viajar a través de tres mundos mágicos, esquivar trampas de pinchos, evitar proyectiles de caramelos y aplastadores pesados para derrotar al Dr. Cocoa en su propia fortaleza en el Nivel 3. ¡Solo ella puede devolver los colores pasteles a Lollipop Valley!", body_style))
    story.append(Spacer(1, 5))
    
    # 3. Niveles y Escenas
    story.append(Paragraph("3. Estructura de Niveles y Escenas", h1_style))
    
    story.append(Paragraph("<b>Escena de Menú Principal (MainMenu):</b>", h2_style))
    story.append(Paragraph("Un menú con interfaz rosa translúcida y botones lavanda. Cuenta con música de fondo alegre e introduce las opciones para empezar a jugar, ver controles/opciones y salir del juego.", body_style))
    
    story.append(Paragraph("<b>Nivel 1: Valle de Algodón de Azúcar (Level1):</b>", h2_style))
    story.append(Paragraph("• <i>Objetivo:</i> Cruzar el valle, aprender mecánicas básicas (moverse, saltar, recolectar estrellas).", bullet_style))
    story.append(Paragraph("• <i>Obstáculos y Enemigos:</i> Pinchos de caramelo amargo (ObstacleSpikes) y el Patrullero de chocolate (EnemyPatrol), un enemigo terrestre que camina horizontalmente.", bullet_style))
    story.append(Paragraph("• <i>Penalización:</i> Si muere aquí, se muestra la UI de derrota y se reinicia el nivel 1.", bullet_style))
    
    story.append(Paragraph("<b>Nivel 2: Las Nubes de Fresa (Level2):</b>", h2_style))
    story.append(Paragraph("• <i>Objetivo:</i> Atravesar plataformas móviles suspendidas en el cielo.", bullet_style))
    story.append(Paragraph("• <i>Obstáculos:</i> Globo Flotante (EnemyFlyer - arriba/abajo), Torreta Disparadora (ObstacleShooter) y el Aplastador (ObstacleThwomp) que cae rápido al pasar por debajo.", bullet_style))
    story.append(Paragraph("• <i>Penalización:</i> Quedarse sin vidas o sin tiempo penaliza al jugador <b>regresándolo automáticamente al Nivel 1</b>.", bullet_style))
    
    story.append(Paragraph("<b>Nivel 3: El Castillo de Fresa y Batalla del Jefe (Level3):</b>", h2_style))
    story.append(Paragraph("• <i>Objetivo:</i> Derrotar al gran Jefe Dr. Cocoa.", bullet_style))
    story.append(Paragraph("• <i>Música:</i> La música alegre de fondo cambia dinámicamente a una música de batalla intensa en el Nivel 3.", bullet_style))
    story.append(Paragraph("• <i>El Jefe (BossController):</i> Tiene 3 puntos de vida (HP). Se desplaza horizontalmente y salta periódicamente. Con cada golpe recibido, se enfurece parpadeando en rojo y acelera su velocidad.", bullet_style))
    story.append(Paragraph("• <i>Penalización/Victoria:</i> Al derrotarlo, el juego termina en Victoria. Al morir, el jugador <b>es penalizado regresándolo al Nivel 1</b>.", bullet_style))
    story.append(Spacer(1, 5))
    
    # 4. Guion e Interacciones
    story.append(Paragraph("4. Guion y Flujo de Interacciones", h1_style))
    story.append(Paragraph("• <i>Estética Rosa Pastel:</i> El script AestheticManager tiñe dinámicamente el fondo de la cámara y aplica colores pasteles con sombras de alto contraste a la UI del juego en tiempo de ejecución de manera automatizada.", bullet_style))
    story.append(Paragraph("• <i>Recolección de Objetos:</i> Al tocar estrellas, se suma puntaje al HUD y suena una campanilla dulce.", bullet_style))
    story.append(Paragraph("• <i>Golpear Bloques:</i> Al saltar y chocar con el bloque desde abajo, este rebota físicamente y arroja una estrella coleccionable, cambiando su textura a bloque de piedra inactivo.", bullet_style))
    story.append(Paragraph("• <i>Muerte del personaje:</i> Si el jugador muere, se congela el movimiento, se despliega el panel de derrota con el texto '¡Te quedaste sin vidas! Regresando al Nivel 1...' y se reproduce una melodía melancólica.", bullet_style))
    story.append(Spacer(1, 5))

    # 5. Scripts Técnicos
    story.append(Paragraph("5. Tabla de Scripts Implementados", h1_style))
    data = [
        ["Script", "Responsabilidad Principal"],
        ["GameManager.cs", "Estado de juego, contador de vidas y temporizador, progresión y retroceso de niveles."],
        ["AestheticManager.cs", "Aplicación dinámica de colores pasteles en UI, botones y color de fondo de cámara."],
        ["PlayerMovement.cs", "Control de movimiento, salto, rebote (stomp) sobre enemigos y recepción de daño."],
        ["AudioManager.cs", "Control y persistencia de música de fondo, música del jefe y efectos de sonido."],
        ["EnemyPatrol.cs", "IA de enemigo terrestre tipo Goomba con giros ante bordes y paredes."],
        ["EnemyFlyer.cs", "Enemigo aéreo que flota en patrón senoidal vertical u horizontal."],
        ["ObstacleSpikes.cs", "Trampa estática que hiere al jugador al contacto."],
        ["ObstacleShooter.cs / Projectile.cs", "Torreta estática que dispara proyectiles que viajan y hieren al jugador."],
        ["ObstacleThwomp.cs", "Bloque pesado que cae con gravedad al detectar al jugador abajo y luego sube."],
        ["BossController.cs", "IA de jefe con 3 vidas, saltos y enfado por velocidad escalonada en Nivel 3."],
        ["CollectibleItem.cs / DestructibleBlock.cs", "Items coleccionables y bloques golpeables estilo Mario con efectos."]
    ]
    
    t = Table(data, colWidths=[120, 410])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (1,0), c_accent),
        ('TEXTCOLOR', (0,0), (1,0), colors.white),
        ('FONTNAME', (0,0), (1,0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0,0), (-1,-1), 4),
        ('TOPPADDING', (0,0), (-1,-1), 4),
        ('GRID', (0,0), (-1,-1), 0.5, colors.grey),
        ('BACKGROUND', (0,1), (-1,-1), c_bg_panel),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('FONTNAME', (0,1), (-1,-1), 'Helvetica'),
        ('FONTSIZE', (0,0), (-1,-1), 8.5),
    ]))
    story.append(t)
    story.append(Spacer(1, 10))
    
    # 6. Enlaces
    story.append(Paragraph("6. Enlaces del Proyecto", h1_style))
    story.append(Paragraph("<b>Repositorio GitHub:</b> <font color='blue'><u>https://github.com/Usuario/PastelDreamWorld-2D</u></font>", body_style))
    story.append(Paragraph("<b>Publicación Itch.io:</b> <font color='blue'><u>https://usuario.itch.io/pastel-dream-world</u></font>", body_style))
    
    doc.build(story)
    print(f"¡PDF generado con éxito como '{pdf_filename}'!")

if __name__ == '__main__':
    generate()
