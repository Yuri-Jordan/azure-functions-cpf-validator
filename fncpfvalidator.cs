using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace hands_on___azure_functions___validar_cpf
{
    public static class fncpfvalidator
    {
        [FunctionName("fncpfvalidator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data == null)
            {
                return new BadRequestObjectResult("Please pass a cpf object in the request body.");
            }

            string cpf = "";

            if (data?.cpf != null)
            {
                cpf = data.cpf;
            }

            string responseMessage;
            if (IsValidCpf(cpf))
            {
                responseMessage = $"The CPF {cpf} is valid.";
            }
            else
            {
                responseMessage = $"The CPF {cpf} is invalid.";
            }

            return new OkObjectResult(responseMessage);
        }

        private static bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = Regex.Replace(cpf, "[^0-9]", "");

            if (cpf.Length != 11)
                return false;

            bool allDigitsSame = true;
            for (int i = 1; i < 11 && allDigitsSame; i++)
            {
                if (cpf[i] != cpf[0])
                    allDigitsSame = false;
            }

            if (allDigitsSame || cpf == "12345678909")
                return false;

            int[] numbers = new int[11];
            for (int i = 0; i < 11; i++)
                numbers[i] = int.Parse(cpf[i].ToString());
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (10 - i) * numbers[i];

            int result = sum % 11;
            if (result == 1 || result == 0)
            {
                if (numbers[9] != 0)
                    return false;
            }
            else if (numbers[9] != 11 - result)
                return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (11 - i) * numbers[i];

            result = sum % 11;
            if (result == 1 || result == 0)
            {
                if (numbers[10] != 0)
                    return false;
            }
            else if (numbers[10] != 11 - result)
                return false;

            return true;
        }
    }
}
