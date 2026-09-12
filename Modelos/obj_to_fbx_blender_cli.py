#!/usr/bin/env python3
"""
Convertidor OBJ a FBX usando Assimp Tool pre-compilado (standalone)
Sin necesidad de instalar nada, solo descargar el ejecutable

Correr con :python obj_to_fbx_blender_cli.py -i ./*origen* -o ./*destino*
"""

import subprocess
import sys
from pathlib import Path
from typing import Tuple
import argparse
import shutil


class AssimpStandaloneConverter:
    """Convertidor usando Assimp Tool ejecutable pre-compilado"""
    
    # Rutas comunes donde podría estar Assimp descargado
    ASSIMP_PATHS = [
        # Si está en PATH (después de agregarlo)
        "assimp",
        
        # Rutas típicas de descarga/instalación en Windows
        "C:\\assimp-5.2.5-win64-Release\\bin\\assimp.exe",
        "C:\\Program Files\\Assimp\\bin\\x64\\assimp.exe",
        "C:\\tools\\assimp\\bin\\assimp.exe",
        ".\\assimp\\bin\\assimp.exe",
        ".\\tools\\assimp.exe",
        # Si se descargó en Descargas
        str(Path.home() / "Downloads" / "assimp" / "bin" / "assimp.exe"),
    ]
    
    def __init__(self, assimp_path: str = None):
        """
        Inicializar convertidor
        
        Args:
            assimp_path: Ruta explícita a assimp.exe (opcional)
        """
        assimp_path = "C:\\Program Files\\Assimp\\bin\\x64\\assimp.exe"
        if assimp_path:
            self.assimp_path = assimp_path
        else:
            self.assimp_path = self._find_assimp()
        
        if not self.assimp_path:
            self._print_download_guide()
            sys.exit(1)
        
        self._verify_tool()
    
    def _find_assimp(self) -> str:
        """Buscar assimp en rutas conocidas"""
        for path in self.ASSIMP_PATHS:
            try:
                # Verificar si es ejecutable
                result = subprocess.run(
                    [path, "--version"],
                    capture_output=True,
                    timeout=5
                )
                if result.returncode == 0:
                    return path
            except (FileNotFoundError, subprocess.TimeoutExpired):
                continue
        
        return None
    
    def _verify_tool(self):
        """Verificar que assimp funciona"""
        try:
            result = subprocess.run(
                [self.assimp_path, "version"],
                capture_output=True,
                text=True,
                timeout=5
            )
            
            if result.returncode != 0:
                raise RuntimeError("Assimp no responde correctamente")
            
            version_info = result.stdout.decode('utf-8', errors='ignore').strip() if isinstance(result.stdout, bytes) else result.stdout.strip()
            print(f"✅ Assimp Tool detectado")
            print(f"📁 Ubicación: {self.assimp_path}")
            print(f"ℹ️  {version_info}\n")
            
        except subprocess.TimeoutExpired:
            raise RuntimeError("Timeout al verificar assimp")
        except Exception as e:
            raise RuntimeError(f"Error al verificar assimp: {e}")
    
    def convert_file(self, obj_file: Path, fbx_file: Path) -> Tuple[bool, str]:
        """
        Convertir un archivo OBJ a FBX
        
        assimp export input.obj output.fbx [opciones]
        
        Returns:
            (éxito, mensaje)
        """
        try:
            # Validar entrada
            if not obj_file.exists():
                return False, f"Archivo no encontrado"
            
            if not obj_file.is_file():
                return False, f"No es un archivo"
            
            # Crear directorio de salida
            fbx_file.parent.mkdir(parents=True, exist_ok=True)
            
            # Ejecutar conversión
            # Opciones: -ptl (print time logs)
            result = subprocess.run(
                [self.assimp_path, "export", str(obj_file), str(fbx_file), "-ptl"],
                capture_output=True,
                text=True,
                timeout=120
            )
            
            # Verificar resultado
            if result.returncode == 0:
                if fbx_file.exists():
                    size = fbx_file.stat().st_size / 1024
                    return True, f"✓ {fbx_file.name} ({size:.1f} KB)"
                else:
                    return False, "Archivo FBX no creado"
            else:
                error_msg = (result.stderr or result.stdout or "Error desconocido")[:100]
                return False, f"{error_msg}"
            
        except subprocess.TimeoutExpired:
            return False, "Timeout (>120 segundos)"
        except FileNotFoundError:
            return False, f"No se encontró: assimp.exe"
        except Exception as e:
            return False, f"Error: {str(e)}"
    
    def convert_batch(self, input_dir: str, output_dir: str, verbose: bool = False):
        """
        Convertir todos los archivos OBJ en un directorio
        
        Args:
            input_dir: Directorio con archivos OBJ
            output_dir: Directorio de salida para FBX
            verbose: Mostrar más detalles
        """
        input_path = Path(input_dir).resolve()
        output_path = Path(output_dir).resolve()
        
        # Validar directorio de entrada
        if not input_path.exists():
            print(f"❌ Error: Directorio no existe: {input_path}")
            return False
        
        if not input_path.is_dir():
            print(f"❌ Error: No es un directorio: {input_path}")
            return False
        
        # Buscar archivos OBJ
        obj_files = sorted(input_path.glob("**/*.obj"))
        
        if not obj_files:
            print(f"⚠️  No se encontraron archivos .obj en: {input_path}")
            return False
        
        # Header
        print("=" * 70)
        print("CONVERTIDOR OBJ → FBX (Assimp Tool)")
        print("=" * 70)
        print(f"Entrada:  {input_path}")
        print(f"Salida:   {output_path}")
        print(f"Archivos: {len(obj_files)}")
        print("=" * 70)
        print()
        
        # Estadísticas
        stats = {
            "total": len(obj_files),
            "converted": 0,
            "failed": 0,
            "errors": []
        }
        
        # Procesar cada archivo
        for i, obj_file in enumerate(obj_files, 1):
            # Calcular ruta relativa para mantener estructura
            relative_path = obj_file.relative_to(input_path)
            fbx_file = output_path / relative_path.with_suffix(".fbx")
            
            # Mostrar progreso
            print(f"[{i}/{stats['total']}] {relative_path}")
            
            # Convertir
            success, message = self.convert_file(obj_file, fbx_file)
            
            if success:
                print(f"  {message}")
                stats["converted"] += 1
            else:
                print(f"  {message}")
                stats["failed"] += 1
                stats["errors"].append((str(relative_path), message))
            
            # Espaciado en modo verbose
            if verbose:
                print()
        
        # Resumen final
        self._print_summary(stats, verbose)
        
        return stats["failed"] == 0
    
    def _print_summary(self, stats: dict, verbose: bool):
        """Imprimir resumen de conversión"""
        print()
        print("=" * 70)
        print("RESUMEN")
        print("=" * 70)
        print(f"Total procesados:  {stats['total']}")
        print(f"Convertidos:    {stats['converted']}")
        print(f"Fallidos:       {stats['failed']}")
        
        if stats["total"] > 0:
            success_rate = (stats["converted"] / stats["total"] * 100)
            print(f"📈 Tasa éxito:     {success_rate:.1f}%")
        
        if stats["errors"] and (verbose or stats["failed"] > 0):
            print("\n❌ Errores detallados:")
            for file_name, error in stats["errors"][:10]:
                print(f"  • {file_name}")
                print(f"    {error}")
            
            if len(stats["errors"]) > 10:
                print(f"  ... y {len(stats['errors']) - 10} errores más")
        
        print("=" * 70)
        
        if stats["failed"] == 0:
            print("✨ ¡Conversión completada exitosamente!")
        else:
            print(f"⚠️  Se encontraron {stats['failed']} errores durante la conversión")
        
        print()
    
    def _print_download_guide(self):
        """Guía para descargar Assimp"""
        print("""❌ ERROR: No se encontró Assimp""")


def main():
    """Función principal con CLI"""
    parser = argparse.ArgumentParser(
        description="Convertidor OBJ a FBX usando Assimp Tool",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ejemplos de uso:

  python convert_with_assimp_standalone.py -i ./Striped -o ./FBX
  python convert_with_assimp_standalone.py -i ./modelos/obj -o ./modelos/fbx -v
  python convert_with_assimp_standalone.py -i ./Striped -o ./FBX -a "C:\\assimp\\bin\\assimp.exe"

Prerequisitos:
  • Descargar assimp-5.2.5-win64-Release.zip desde https://www.assimp.org
  • Extraer en cualquier carpeta
        """
    )
    
    parser.add_argument(
        "-i", "--input",
        required=True,
        help="Directorio con archivos OBJ"
    )
    parser.add_argument(
        "-o", "--output",
        required=True,
        help="Directorio de salida para archivos FBX"
    )
    parser.add_argument(
        "-a", "--assimp",
        default=None,
        help="Ruta explícita a assimp.exe (si no está en rutas conocidas)"
    )
    parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Modo verbose (más detalles)"
    )
    
    args = parser.parse_args()
    
    try:
        # Crear convertidor
        converter = AssimpStandaloneConverter(assimp_path=args.assimp)
        
        # Realizar conversión
        success = converter.convert_batch(
            input_dir=args.input,
            output_dir=args.output,
            verbose=args.verbose
        )
        
        # Retornar código de salida
        return 0 if success else 1
        
    except KeyboardInterrupt:
        print("\n\n⚠️  Conversión cancelada por el usuario")
        return 130
        
    except Exception as e:
        print(f"\n❌ Error: {e}")
        if '--verbose' in sys.argv:
            import traceback
            traceback.print_exc()
        return 1


if __name__ == "__main__":
    sys.exit(main())