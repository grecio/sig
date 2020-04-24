using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class Artefato
    {
        public static DAL.DsArtefato .BriefingArtefatoDataTable Listar(int idbriefing)
        {

            Validador.Validar(idbriefing > 0, "Informe o briefing.");

            using (var adp = new DAL.DsArtefatoTableAdapters.BriefingArtefatoTableAdapter())
            {
                return adp.Listar(idbriefing);
            }

        }
    }
}
