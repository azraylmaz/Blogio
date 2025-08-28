using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogio.Blog
{
    public enum DraftStatus
    {
        Editing = 0,  // yazar düzenliyor
        Submitted = 1,  // admin onayında (kilitli)
        Approved = 2,  // arşiv amaçlı bilgi
        Rejected = 3   // reddedildi (yazar tekrar düzenleyebilir)
    }
}
