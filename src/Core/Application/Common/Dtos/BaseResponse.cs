using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Dtos;
public class BaseResponse<T>
{
    public string Message { get; set; }
    public bool Status { get; set; }
    public T Data { get; set; }
}
